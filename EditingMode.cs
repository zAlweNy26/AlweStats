using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AlweStats {
    public static class EditingMode {
        private static GameObject resetObj = null;
        private static bool isEditing = false;
        private static List<GameObject> templateObjs = null;
        private static List<Block> blockObjs = null;
        private static Vector3 lastMousePos = Vector3.zero;
        private static string currentlyDragging = "";
        private static float lastScrollPos = 0f;

        public static void Start(List<Block> blocks) {
            templateObjs = new();
            blockObjs = blocks;
            foreach (Block b in blocks) {
                GameObject templateObj = UnityEngine.Object.Instantiate(b.GetGameObject(), b.GetTransform());
                templateObj.name = $"{b.GetName()}Template";
                templateObj.transform.SetParent(b.GetParent());
                templateObj.GetComponent<VerticalLayoutGroup>().padding = new RectOffset(0, 0, 0, 0);
                UnityEngine.Object.Destroy(templateObj.GetComponent<ContentSizeFitter>());
                templateObj.GetComponent<Image>().color = new Color(0.25f, 0.25f, 0.25f, 1f);
                Text templateText = templateObj.GetComponentInChildren<Text>();
                templateText.text = b.GetName();
                templateText.color = Color.white;
                templateText.alignment = TextAnchor.MiddleCenter;
                templateText.resizeTextForBestFit = true;
                templateObj.SetActive(false);
                templateObjs.Add(templateObj);
            }
        }

        public static void Update() {
            if (!isEditing) return;
            Vector3 mousePos = Input.mousePosition;
            float scrollPos = Input.mouseScrollDelta.y;
            if (lastMousePos == Vector3.zero) lastMousePos = mousePos;
            if (lastScrollPos == 0f) lastScrollPos = scrollPos;
            if (Input.GetKey(KeyCode.LeftControl)) {
                foreach (GameObject g in templateObjs) {
                    RectTransform currentRect = g.GetComponent<RectTransform>();
                    if (RectTransformUtility.RectangleContainsScreenPoint(currentRect, mousePos)) {
                        float scaledValueX = (float) Math.Round(Mathf.Abs(currentRect.sizeDelta.x + (scrollPos * 2)), 2);
                        float scaledValueY = (float) Math.Round(Mathf.Abs(currentRect.sizeDelta.y + (scrollPos * 2)), 2);
                        currentRect.sizeDelta = new Vector2(scaledValueX, scaledValueY);
                        g.GetComponentInChildren<Text>().fontSize += (int) Math.Round(scrollPos / 2);
                    }
                }
            }
            if (Input.GetKey(KeyCode.Mouse0) && !Input.GetKey(KeyCode.LeftControl)) {
                if (currentlyDragging != "") {
                    Transform current = Hud.instance.m_rootObject.transform.Find(currentlyDragging);
                    if (current) {
                        RectTransform currentRect = current.GetComponent<RectTransform>();
                        currentRect.position = mousePos;
                    }
                } else {
                    foreach (GameObject g in templateObjs) {
                        RectTransform currentRect = g.GetComponent<RectTransform>();
                        if (RectTransformUtility.RectangleContainsScreenPoint(currentRect, mousePos)) {
                            currentRect.position = mousePos;
                            currentlyDragging = g.name;
                            break;
                        }
                    }
                }
            } else currentlyDragging = "";
            lastMousePos = mousePos;
            lastScrollPos = scrollPos;
        }

        public static void OnPress() {
            isEditing = !isEditing;
            if (templateObjs == null) return;
            if (isEditing) {
                //Debug.Log("Editing mode : ON !");
                foreach (GameObject g in templateObjs) {
                    Transform original = g.transform.parent.Find(g.name.Replace("Template", ""));
                    RectTransform originalRect = original.GetComponent<RectTransform>();
                    RectTransform templateRect = g.GetComponent<RectTransform>();
                    templateRect.pivot = templateRect.anchorMin = templateRect.anchorMax = originalRect.pivot;
                    templateRect.sizeDelta = originalRect.sizeDelta;
                    templateRect.anchoredPosition = originalRect.anchoredPosition;
                    templateRect.position = originalRect.position;
                    if (original.gameObject.activeSelf) g.SetActive(true);
                }
            } else if (!isEditing) {
                //Debug.Log("Editing mode : OFF !");
                foreach (GameObject g in templateObjs) {
                    Transform original = g.transform.parent.Find(g.name.Replace("Template", ""));
                    RectTransform originalRect = original.GetComponent<RectTransform>();
                    RectTransform templateRect = g.GetComponent<RectTransform>();
                    Text templateText = g.GetComponentInChildren<Text>();
                    Text originalText = original.GetComponentInChildren<Text>();
                    originalText.fontSize = templateText.fontSize;
                    originalRect.position = templateRect.position;
                    g.SetActive(false);
                }
            }
        }

        public static void Destroy(List<Block> blocks) {
            foreach (Block b in blocks) {
                b.SetSize(b.GetText().fontSize);
                b.SetPosition(b.GetRect().pivot);
                b.SetMargin(b.GetRect().anchoredPosition);
            }
            Main.config.Save();
            Main.ReloadConfig();
        }

        private static void Reset() {
            isEditing = true;
            OnPress();
            foreach (Block b in blockObjs) {
                b.SetSize((int) b.GetConfigValue<int>(b.GetName(), "Size").DefaultValue);
                b.SetPosition(b.GetConfigValue<string>(b.GetName(), "Position").DefaultValue.ToString());
                b.SetMargin(b.GetConfigValue<string>(b.GetName(), "Margin").DefaultValue.ToString());
            }
            Main.config.Save();
        }

        public static void ShowButton() {
            if (resetObj == null && Menu.instance.m_menuDialog.Find("ResetAlweStats") == null) {
                GameObject originalObj = Menu.instance.m_menuDialog.Find("Close").gameObject;
                resetObj = UnityEngine.Object.Instantiate(originalObj, originalObj.transform);
                resetObj.name = "ResetAlweStats";
                resetObj.transform.SetParent(originalObj.transform.parent);
                resetObj.transform.localPosition = new Vector3(0, originalObj.transform.localPosition.y - 40f, 0f);
                resetObj.GetComponentInChildren<Text>().text = "Reset AlweStats";
                resetObj.transform.Find("LeftKnot").localPosition = new Vector3(-110f, 0f, 0f);
                resetObj.transform.Find("RightKnot").localPosition = new Vector3(110f, 0f, 0f);
                resetObj.GetComponent<Button>().onClick.RemoveAllListeners();
                resetObj.GetComponent<Button>().onClick.AddListener(Reset);
                resetObj.SetActive(true);
            }
        }
    }
}
