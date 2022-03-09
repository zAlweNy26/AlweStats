using BepInEx.Configuration;
using System;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace AlweStats {
    public class Block {
        private readonly GameObject blockObj = null;

        public Block(string name, string color, int size, string position, string margin, string align = "") {
            blockObj = new GameObject(name);
            blockObj.transform.SetParent(Hud.instance.m_rootObject.transform);
            blockObj.AddComponent<RectTransform>();
            RectTransform statsRect = blockObj.GetComponent<RectTransform>();
            statsRect.anchorMax = statsRect.anchorMin = statsRect.pivot = StringToVector(position);
            statsRect.anchoredPosition = new Vector2(0f, 0f);

            Canvas canvas = blockObj.AddComponent<Canvas>();
            GameObject backgroundObj = new("Background");
            backgroundObj.transform.SetParent(canvas.transform);
            backgroundObj.AddComponent<Image>().color = StringToColor(Main.blocksBackgroundColor.Value);
            RectTransform backgroundRect = backgroundObj.GetComponent<RectTransform>();
            backgroundRect.anchorMax = backgroundRect.anchorMin = backgroundRect.pivot = StringToVector(position);
            backgroundRect.anchoredPosition = StringToVector(margin);

            GameObject textObj = new("Content");
            textObj.transform.SetParent(canvas.transform);
            Text blockText = textObj.AddComponent<Text>();
            blockText.color = StringToColor(color);
            blockText.font = MessageHud.instance.m_messageCenterText.font;
            blockText.fontSize = size;
            blockText.enabled = true;
            if (align != "") {
                Enum.TryParse(align, out TextAnchor textAlignment);
                blockText.alignment = textAlignment;
            }
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMax = textRect.anchorMin = textRect.pivot = StringToVector(position);
            textRect.anchoredPosition = StringToVector(margin);

            blockObj.SetActive(true);
        }

        public bool IsActive() {
            return blockObj.activeSelf;
        }

        public bool IsActiveAndAlsoParents() {
            return blockObj.activeInHierarchy;
        }

        public void SetActive(bool active) {
            blockObj.SetActive(active);
        }

        public string GetName() {
            return blockObj.name;
        }

        public Transform GetParent() {
            return blockObj.transform.parent;
        }

        public string GetText() {
            return blockObj.GetComponentInChildren<Text>().text;
        }

        public void SetText(string text) {
            blockObj.GetComponentInChildren<Text>().text = text;
        }

        public GameObject GetGameObject() {
            return blockObj;
        }

        public Transform GetTransform() {
            return blockObj.transform;
        }

        public RectTransform GetBackgroundRect() {
            return blockObj.transform.Find("Background").GetComponent<RectTransform>();
        }

        public RectTransform GetContentRect() {
            return blockObj.transform.Find("Content").GetComponent<RectTransform>();
        }

        public void SetPosition(string position) {
            RectTransform bgRect = GetBackgroundRect();
            bgRect.anchorMax = bgRect.anchorMin = bgRect.pivot = StringToVector(position);
            RectTransform ctRect = GetContentRect();
            ctRect.anchorMax = ctRect.anchorMin = ctRect.pivot = StringToVector(position);
            SetConfigValue(GetName(), "Position", position);
        }

        public void SetPosition(Vector2 position) {
            RectTransform bgRect = GetBackgroundRect();
            bgRect.anchorMax = bgRect.anchorMin = bgRect.pivot = position;
            RectTransform ctRect = GetContentRect();
            ctRect.anchorMax = ctRect.anchorMin = ctRect.pivot = position;
            SetConfigValue(GetName(), "Position", VectorToString(position));
        }

        public void SetMargin(string margin) {
            RectTransform bgRect = GetBackgroundRect();
            bgRect.anchoredPosition = StringToVector(margin);
            RectTransform ctRect = GetContentRect();
            ctRect.anchoredPosition = StringToVector(margin);
            SetConfigValue(GetName(), "Margin", margin);
        }

        public void SetMargin(Vector2 margin) {
            RectTransform bgRect = GetBackgroundRect();
            bgRect.anchoredPosition = margin;
            RectTransform ctRect = GetContentRect();
            ctRect.anchoredPosition = margin;
            SetConfigValue(GetName(), "Margin", VectorToString(margin));
        }

        public ConfigEntry<T> GetConfigValue<T>(string section, string key) {
            Main.config.TryGetEntry(new ConfigDefinition(section, key), out ConfigEntry<T> entry);
            return entry;
        }

        public void SetConfigValue<T>(string section, string key, T value) {
            Main.config.TryGetEntry(new ConfigDefinition(section, key), out ConfigEntry<T> entry);
            entry.Value = value;
        }

        public string VectorToString(Vector2 v) {
            string x = v.x.ToString("0.##", CultureInfo.InvariantCulture);
            string y = v.y.ToString("0.##", CultureInfo.InvariantCulture);
            return $"{x}, {y}";
        }

        public Vector2 StringToVector(string s) {
            string[] values = Regex.Replace(s, @"\s+", "").Split(',');
            float x = float.Parse(values[0], CultureInfo.InvariantCulture);
            float y = float.Parse(values[1], CultureInfo.InvariantCulture);
            return new Vector2(x, y);
        }

        public Color StringToColor(string s) {
            string[] values = Regex.Replace(s, @"\s+", "").Split(',');
            return new Color(
                Mathf.Clamp01(float.Parse(values[0], CultureInfo.InvariantCulture) / 255f),
                Mathf.Clamp01(float.Parse(values[1], CultureInfo.InvariantCulture) / 255f),
                Mathf.Clamp01(float.Parse(values[2], CultureInfo.InvariantCulture) / 255f),
                Mathf.Clamp01(float.Parse(values[3], CultureInfo.InvariantCulture) / 255f)
            );
        }
    }
}
