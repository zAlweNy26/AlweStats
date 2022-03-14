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
            blockObj = new(name);
            blockObj.transform.SetParent(Hud.instance.m_rootObject.transform);
            blockObj.AddComponent<RectTransform>();
            blockObj.AddComponent<Image>().color = StringToColor(Main.blocksBackgroundColor.Value);
            RectTransform statsRect = blockObj.GetComponent<RectTransform>();
            statsRect.anchorMax = statsRect.anchorMin = statsRect.pivot = StringToVector(position);
            statsRect.anchoredPosition = StringToVector(margin);

            VerticalLayoutGroup group = blockObj.AddComponent<VerticalLayoutGroup>();
            group.padding = StringToPadding(Main.blocksPadding.Value);
            ContentSizeFitter contentFitter = blockObj.AddComponent<ContentSizeFitter>();
            contentFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            GameObject textObj = new("Content");
            textObj.transform.SetParent(group.transform);
            Text blockText = textObj.AddComponent<Text>();
            blockText.color = StringToColor(color);
            blockText.font = MessageHud.instance.m_messageCenterText.font;
            blockText.fontSize = size;
            blockText.enabled = true;
            TextAnchor textAlignment;
            if (Enum.TryParse(align, out textAlignment)) blockText.alignment = textAlignment;
            else blockText.alignment = TextAnchor.MiddleCenter;
            textObj.AddComponent<Outline>();

            blockObj.SetActive(true);
        }

        public bool IsActive() {
            return blockObj.activeSelf;
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

        public RectTransform GetRect() {
            return blockObj.GetComponent<RectTransform>();
        }

        public void SetPosition(string position) {
            RectTransform statsRect = blockObj.GetComponent<RectTransform>();
            statsRect.anchorMax = statsRect.anchorMin = statsRect.pivot = StringToVector(position);
            SetConfigValue(GetName(), "Position", position);
        }

        public void SetPosition(Vector2 position) {
            RectTransform statsRect = blockObj.GetComponent<RectTransform>();
            statsRect.anchorMax = statsRect.anchorMin = statsRect.pivot = position;
            SetConfigValue(GetName(), "Position", VectorToString(position));
        }

        public void SetMargin(string margin) {
            RectTransform statsRect = blockObj.GetComponent<RectTransform>();
            statsRect.anchoredPosition = StringToVector(margin);
            SetConfigValue(GetName(), "Margin", margin);
        }

        public void SetMargin(Vector2 margin) {
            RectTransform statsRect = blockObj.GetComponent<RectTransform>();
            statsRect.anchoredPosition = margin;
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

        public RectOffset StringToPadding(string s) {
            string[] values = Regex.Replace(s, @"\s+", "").Split(',');
            int left = int.Parse(values[0]);
            int right = int.Parse(values[1]);
            int top = int.Parse(values[2]);
            int bottom = int.Parse(values[3]);
            return new RectOffset(left, right, top, bottom);
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
