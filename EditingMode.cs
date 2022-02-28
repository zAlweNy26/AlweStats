using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AlweStats {
    public static class EditingMode {
        private static GameObject editObj = null;
        private static bool isEditing = false;
        private static readonly List<GameObject> templateObjs = new();
        private static List<Block> blockObjs = new();
        private static Vector3 lastMousePos = Vector3.zero;
        private static string currentlyDragging = "";

        public static void Start(List<Block> blocks) {
            blockObjs = blocks;
            foreach (Block b in blocks) {
                GameObject templateObj = new($"{b.GetName()}Template");
                templateObj.transform.SetParent(b.GetParent());
                templateObj.AddComponent<RectTransform>();
                Canvas canvas = templateObj.AddComponent<Canvas>();
                GameObject backgroundObj = new("Background");
                backgroundObj.transform.SetParent(canvas.transform);
                backgroundObj.AddComponent<Image>().color = new Color(0.25f, 0.25f, 0.25f, 0.5f);
                GameObject textObj = new("Title");
                textObj.transform.SetParent(canvas.transform);
                Text templateText = textObj.AddComponent<Text>();
                templateText.text = b.GetName();
                templateText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                templateText.fontSize = 24;
                templateText.alignment = TextAnchor.MiddleCenter;
                templateText.resizeTextForBestFit = true;
                templateObj.SetActive(false);
                templateObjs.Add(templateObj);
            }
        }

        public static void Update() {
            if (Input.GetKeyDown(Main.toggleEditMode.Value)) OnPress();
            if (!isEditing) return;
            float gameScale = GameObject.Find("GUI").GetComponent<CanvasScaler>().scaleFactor;
            Vector3 mousePos = Input.mousePosition;
            if (lastMousePos == Vector3.zero) lastMousePos = mousePos;
            if (Input.GetKey(KeyCode.Mouse0)) {
                if (currentlyDragging != "") {
                    Transform current = Hud.instance.transform.Find($"hudroot/{currentlyDragging}");
                    current.position = lastMousePos / gameScale;
                } else {
                    foreach (GameObject g in templateObjs) {
                        if (RectTransformUtility.RectangleContainsScreenPoint(g.GetComponent<RectTransform>(), mousePos)) {
                            g.transform.position = lastMousePos / gameScale;
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
            if (isEditing) {
                //Debug.Log("Editing mode : ON !");
                foreach (GameObject g in templateObjs) {
                    RectTransform originalRT = blockObjs.Find(b => b.GetName() == g.name.Replace("Template", "")).GetRect();
                    RectTransform templateRT = g.GetComponent<RectTransform>();
                    templateRT.pivot = originalRT.pivot;
                    templateRT.anchorMin = originalRT.anchorMin;
                    templateRT.anchorMax = originalRT.anchorMax;
                    templateRT.offsetMin = originalRT.offsetMin;
                    templateRT.offsetMax = originalRT.offsetMax;
                    templateRT.sizeDelta = originalRT.sizeDelta;
                    templateRT.anchoredPosition = originalRT.anchoredPosition;
                    templateRT.position = originalRT.position;
                    templateRT.localEulerAngles = originalRT.localEulerAngles;
                    foreach (Transform t in g.transform) {
                        RectTransform rt = t.GetComponent<RectTransform>();
                        rt.pivot = originalRT.pivot;
                        rt.anchorMin = originalRT.anchorMin;
                        rt.anchorMax = originalRT.anchorMax;
                        rt.offsetMin = originalRT.offsetMin;
                        rt.offsetMax = originalRT.offsetMax;
                        rt.sizeDelta = originalRT.sizeDelta;
                        rt.anchoredPosition = originalRT.anchoredPosition;
                        rt.position = originalRT.position;
                        rt.localEulerAngles = originalRT.localEulerAngles;
                    }
                    g.SetActive(true);
                }
            } else if (!isEditing) {
                //Debug.Log("Editing mode : OFF !");
                foreach (GameObject g in templateObjs) {
                    RectTransform gameOriginal = blockObjs.Find(b => b.GetName() == g.name.Replace("Template", "")).GetRect();
                    gameOriginal.position = g.transform.position;
                    g.SetActive(false);
                }
            }
        }

        public static void Destroy() {
            foreach (GameObject g in templateObjs) Object.Destroy(g);
            templateObjs.Clear();
            foreach (Block b in blockObjs) {
                b.SetPosition(b.GetRect().pivot);
                b.SetMargin(b.GetRect().anchoredPosition);
            }
            blockObjs.Clear();
            Main.ReloadConfig();
        }

        private static void Reset() {
            isEditing = false;
            foreach (Block b in blockObjs) {
                b.SetPosition(b.GetConfigValue<string>(b.GetName(), "Position").DefaultValue.ToString());
                b.SetMargin(b.GetConfigValue<string>(b.GetName(), "Margin").DefaultValue.ToString());
            }
            Destroy();
        }

        public static void ShowButton() {
            if (editObj == null) {
                GameObject originalObj = Menu.instance.m_menuDialog.Find("Close").gameObject;
                editObj = Object.Instantiate(originalObj, originalObj.transform);
                editObj.name = "ResetAlweStats";
                editObj.transform.SetParent(originalObj.transform.parent);
                editObj.transform.localPosition = new Vector3(0, originalObj.transform.localPosition.y - 40f, 0f);
                editObj.GetComponentInChildren<Text>().text = "Reset AlweStats";
                editObj.transform.Find("LeftKnot").localPosition = new Vector3(-110f, 0f, 0f);
                editObj.transform.Find("RightKnot").localPosition = new Vector3(110f, 0f, 0f);
                editObj.GetComponent<Button>().onClick.RemoveAllListeners();
                editObj.GetComponent<Button>().onClick.AddListener(Reset);
                editObj.SetActive(true);
            }
        }
    }
}
