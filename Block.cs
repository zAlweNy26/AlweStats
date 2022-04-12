using BepInEx.Configuration;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace AlweStats {
    public class Block {
        private readonly GameObject blockObj = null;

        public Block(string name, string color, int size, string position, string margin, string align = "") {
            blockObj = new(name);
            blockObj.transform.SetParent(Hud.instance.m_rootObject.transform);
            blockObj.AddComponent<RectTransform>();
            blockObj.AddComponent<Image>().color = Utilities.StringToColor(Main.blocksBackgroundColor.Value);
            RectTransform statsRect = blockObj.GetComponent<RectTransform>();
            statsRect.anchorMax = statsRect.anchorMin = statsRect.pivot = Utilities.StringToVector(position);
            statsRect.anchoredPosition = Utilities.StringToVector(margin);

            VerticalLayoutGroup group = blockObj.AddComponent<VerticalLayoutGroup>();
            group.padding = Utilities.StringToPadding(Main.blocksPadding.Value);
            ContentSizeFitter contentFitter = blockObj.AddComponent<ContentSizeFitter>();
            contentFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            GameObject textObj = new("Content");
            textObj.transform.SetParent(group.transform);
            Text blockText = textObj.AddComponent<Text>();
            blockText.color = Utilities.StringToColor(color);
            blockText.font = MessageHud.instance.m_messageCenterText.font;
            blockText.fontSize = size;
            blockText.enabled = true;
            TextAnchor textAlignment;
            if (Enum.TryParse(align, out textAlignment)) blockText.alignment = textAlignment;
            else blockText.alignment = TextAnchor.MiddleCenter;
            Outline textOutline = textObj.AddComponent<Outline>();
            textOutline.effectColor = Color.black;

            blockObj.SetActive(true);
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
            statsRect.anchorMax = statsRect.anchorMin = statsRect.pivot = Utilities.StringToVector(position);
            SetConfigValue(GetName(), "Position", position);
        }

        public void SetPosition(Vector2 position) {
            RectTransform statsRect = blockObj.GetComponent<RectTransform>();
            statsRect.anchorMax = statsRect.anchorMin = statsRect.pivot = position;
            SetConfigValue(GetName(), "Position", Utilities.VectorToString(position));
        }

        public void SetMargin(string margin) {
            RectTransform statsRect = blockObj.GetComponent<RectTransform>();
            statsRect.anchoredPosition = Utilities.StringToVector(margin);
            SetConfigValue(GetName(), "Margin", margin);
        }

        public void SetMargin(Vector2 margin) {
            RectTransform statsRect = blockObj.GetComponent<RectTransform>();
            statsRect.anchoredPosition = margin;
            SetConfigValue(GetName(), "Margin", Utilities.VectorToString(margin));
        }

        public ConfigEntry<T> GetConfigValue<T>(string section, string key) {
            Main.config.TryGetEntry(new ConfigDefinition(section, key), out ConfigEntry<T> entry);
            return entry;
        }

        public void SetConfigValue<T>(string section, string key, T value) {
            Main.config.TryGetEntry(new ConfigDefinition(section, key), out ConfigEntry<T> entry);
            entry.Value = value;
        }
    }
}
