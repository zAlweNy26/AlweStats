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
            if (lastMousePos == Vector3.zero) lastMousePos = mousePos;
            if (Input.GetKey(KeyCode.Mouse0)) {
                if (currentlyDragging != "") {
                    Transform current = Hud.instance.m_rootObject.transform.Find(currentlyDragging);
                    if (current) {
                        RectTransform currentRect = current.GetComponent<RectTransform>();
                        current.position = mousePos - new Vector3(currentRect.sizeDelta.x / 2f, currentRect.sizeDelta.y / 8f, 0f);
                    }
                } else {
                    foreach (GameObject g in templateObjs) {
                        if (RectTransformUtility.RectangleContainsScreenPoint(g.GetComponent<RectTransform>(), mousePos)) {
                            RectTransform currentRect = g.GetComponent<RectTransform>();
                            g.transform.position = mousePos - new Vector3(currentRect.sizeDelta.x / 2f, currentRect.sizeDelta.y / 8f, 0f);
                            currentlyDragging = g.name;
                            break;
                        }
                    }
                }
            } else currentlyDragging = "";
            lastMousePos = mousePos;
        }

        public static void OnPress() {
            isEditing = !isEditing;
            if (templateObjs == null) return;
            if (isEditing) {
                //Debug.Log("Editing mode : ON !");
                foreach (GameObject g in templateObjs) {
                    RectTransform original = g.transform.parent.Find(g.name.Replace("Template", "")).GetComponent<RectTransform>();
                    RectTransform rt = g.GetComponent<RectTransform>();
                    rt.pivot = rt.anchorMin = rt.anchorMax = original.pivot;
                    rt.sizeDelta = original.sizeDelta;
                    rt.anchoredPosition = original.anchoredPosition;
                    rt.position = original.position;
                    if (original.gameObject.activeSelf) g.SetActive(true);
                }
            } else if (!isEditing) {
                //Debug.Log("Editing mode : OFF !");
                foreach (GameObject g in templateObjs) {
                    RectTransform original = g.transform.parent.Find(g.name.Replace("Template", "")).GetComponent<RectTransform>();
                    RectTransform rt = g.GetComponent<RectTransform>();
                    original.position = rt.position;
                    g.SetActive(false);
                }
            }
        }

        public static void Destroy(List<Block> blocks) {
            foreach (Block b in blocks) {
                b.SetPosition(b.GetRect().pivot);
                b.SetMargin(b.GetRect().anchoredPosition);
            }
            Main.config.Save();
            Main.ReloadConfig();
        }

        private static void Reset() {
            isEditing = false;
            foreach (Block b in blockObjs) {
                b.SetPosition(b.GetConfigValue<string>(b.GetName(), "Position").DefaultValue.ToString());
                b.SetMargin(b.GetConfigValue<string>(b.GetName(), "Margin").DefaultValue.ToString());
            }
            Main.config.Save();
        }

        public static void ShowButton() {
            if (resetObj == null && Menu.instance.m_menuDialog.Find("ResetAlweStats") == null) {
                GameObject originalObj = Menu.instance.m_menuDialog.Find("Close").gameObject;
                resetObj = Object.Instantiate(originalObj, originalObj.transform);
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
