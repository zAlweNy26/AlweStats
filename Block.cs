using BepInEx.Configuration;
using System;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace AlweStats {
    public class Block {
        private readonly GameObject blockObj = null;

        public Block(string name, string color, int size, string align, string position, string margin) {
            blockObj = new GameObject(name);
            blockObj.transform.SetParent(Hud.instance.transform.Find("hudroot"));
            blockObj.AddComponent<RectTransform>();
            ContentSizeFitter contentFitter = blockObj.AddComponent<ContentSizeFitter>();
            contentFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            Text gameText = blockObj.AddComponent<Text>();
            gameText.color = StringToColor(color);
            gameText.font = MessageHud.instance.m_messageCenterText.font;
            gameText.fontSize = size;
            gameText.enabled = true;
            if (align != "") {
                Enum.TryParse(align, out TextAnchor textAlignment);
                gameText.alignment = textAlignment;
            }
            RectTransform statsRect = blockObj.GetComponent<RectTransform>();
            statsRect.anchorMax = statsRect.anchorMin = statsRect.pivot = StringToVector(position);
            statsRect.anchoredPosition = StringToVector(margin);
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
            return blockObj.GetComponent<Text>().text;
        }

        public void SetText(string text) {
            blockObj.GetComponent<Text>().text = text;
        }

        public RectTransform GetRect() {
            return blockObj.GetComponent<RectTransform>();
        }

        public void SetPosition(string position) {
            RectTransform blockRect = blockObj.GetComponent<RectTransform>();
            blockRect.anchorMax = blockRect.anchorMin = blockRect.pivot = StringToVector(position);
            SetConfigValue(GetName(), "Position", position);
        }

        public void SetPosition(Vector2 position) {
            RectTransform blockRect = blockObj.GetComponent<RectTransform>();
            blockRect.anchorMax = blockRect.anchorMin = blockRect.pivot = position;
            SetConfigValue(GetName(), "Position", VectorToString(position));
        }

        public void SetMargin(string margin) {
            RectTransform blockRect = blockObj.GetComponent<RectTransform>();
            blockRect.anchoredPosition = StringToVector(margin);
            SetConfigValue(GetName(), "Margin", margin);
        }

        public void SetMargin(Vector2 margin) {
            RectTransform blockRect = blockObj.GetComponent<RectTransform>();
            blockRect.anchoredPosition = margin;
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
            string x = v.x.ToString("0.00", CultureInfo.InvariantCulture);
            string y = v.y.ToString("0.00", CultureInfo.InvariantCulture);
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
