using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace AlweStats {
    public static class EditingMode {
        private static GameObject editObj = null;
        private static bool isEditing = false;
        private static readonly Transform[] inGameOriginals = new Transform[4];
        private static readonly List<GameObject> templateObjs = new(4);
        private static Vector3 lastMousePos = Vector3.zero;
        private static string currentlyDragging = "";

        public static void Start() {
            inGameOriginals[0] = Hud.instance.transform.Find("hudroot/GameStats");
            inGameOriginals[1] = Hud.instance.transform.Find("hudroot/WorldStats");
            inGameOriginals[2] = Hud.instance.transform.Find("hudroot/WorldClock");
            inGameOriginals[3] = Hud.instance.transform.Find("hudroot/ShipStats");
            foreach (Transform t in inGameOriginals) {
                GameObject templateObj = new($"{t.name}Template");
                templateObj.transform.SetParent(t.parent);
                templateObj.AddComponent<RectTransform>();
                Canvas canvas = templateObj.AddComponent<Canvas>();
                GameObject backgroundObj = new("Background");
                backgroundObj.transform.SetParent(canvas.transform);
                backgroundObj.AddComponent<Image>().color = new Color(0.25f, 0.25f, 0.25f, 0.5f);
                GameObject textObj = new("Title");
                textObj.transform.SetParent(canvas.transform);
                Text templateText = textObj.AddComponent<Text>();
                templateText.text = t.name;
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
                    RectTransform originalRT = Hud.instance.transform.Find($"hudroot{g.name.Replace("Template", "")}").GetComponent<RectTransform>();
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
                    Transform gameOriginal = Hud.instance.transform.Find($"hudroot{g.name.Replace("Template", "")}");
                    gameOriginal.position = g.transform.position;
                    g.SetActive(false);
                }
            }
        }

        private static string VectorToString(Vector2 v) {
            string x = v.x.ToString("0.00", CultureInfo.InvariantCulture);
            string y = v.y.ToString("0.00", CultureInfo.InvariantCulture);
            return $"{x}, {y}";
        }

        public static void Destroy() {
            foreach (GameObject g in templateObjs) Object.Destroy(g);
            templateObjs.Clear();
            Main.gameStatsPosition.Value = VectorToString(inGameOriginals[0].GetComponent<RectTransform>().pivot);
            Main.worldStatsPosition.Value = VectorToString(inGameOriginals[1].GetComponent<RectTransform>().pivot);
            Main.worldClockPosition.Value = VectorToString(inGameOriginals[2].GetComponent<RectTransform>().pivot);
            Main.shipStatsPosition.Value = VectorToString(inGameOriginals[3].GetComponent<RectTransform>().pivot);
            Main.gameStatsMargin.Value = VectorToString(inGameOriginals[0].GetComponent<RectTransform>().anchoredPosition);
            Main.worldStatsMargin.Value = VectorToString(inGameOriginals[1].GetComponent<RectTransform>().anchoredPosition);
            Main.worldClockMargin.Value = VectorToString(inGameOriginals[2].GetComponent<RectTransform>().anchoredPosition);
            Main.shipStatsMargin.Value = VectorToString(inGameOriginals[3].GetComponent<RectTransform>().anchoredPosition);
        }

        private static void Reset() {
            isEditing = false;
            Vector2[] positions = { new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 1f), new Vector2(1f, 0.5f) };
            Vector2[] margins = { new Vector2(5f, 5f), new Vector2(-5f, 5f), new Vector2(0f, 0f), new Vector2(-5f, 0f) };
            for (int i = 0; i < inGameOriginals.Length; i++) {
                RectTransform originalRT = inGameOriginals[i].GetComponent<RectTransform>();
                originalRT.anchorMax = originalRT.anchorMin = originalRT.pivot = positions[i];
                originalRT.anchoredPosition = margins[i];
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
