using System;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace AlweStats {
    public static class WorldClock {
        private static GameObject clockObj = null;

        public static void Start() {
            clockObj = new GameObject("WorldClock");
            clockObj.transform.SetParent(Hud.instance.transform.Find("hudroot"));
            clockObj.AddComponent<RectTransform>();
            ContentSizeFitter contentFitter = clockObj.AddComponent<ContentSizeFitter>();
            contentFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            Text clockText = clockObj.AddComponent<Text>();
            string[] colors = Regex.Replace(Main.worldClockColor.Value, "\\s+", "").Split(',');
            clockText.color = new(
                Mathf.Clamp01(float.Parse(colors[0], CultureInfo.InvariantCulture) / 255f),
                Mathf.Clamp01(float.Parse(colors[1], CultureInfo.InvariantCulture) / 255f),
                Mathf.Clamp01(float.Parse(colors[2], CultureInfo.InvariantCulture) / 255f),
                Mathf.Clamp01(float.Parse(colors[3], CultureInfo.InvariantCulture) / 255f)
            );
            clockText.font = MessageHud.instance.m_messageCenterText.font;
            clockText.fontSize = Main.worldClockSize.Value;
            clockText.enabled = true;
            RectTransform clockRect = clockObj.GetComponent<RectTransform>();
            string[] positions = Regex.Replace(Main.worldClockPosition.Value, "\\s+", "").Split(',');
            clockRect.anchorMax = clockRect.anchorMin = clockRect.pivot = new(
                float.Parse(positions[0], CultureInfo.InvariantCulture),
                float.Parse(positions[1], CultureInfo.InvariantCulture)
            );
            string[] margins = Regex.Replace(Main.worldClockMargin.Value, @"\s+", "").Split(',');
            clockRect.anchoredPosition = new Vector2(
                float.Parse(margins[0], CultureInfo.InvariantCulture),
                float.Parse(margins[1], CultureInfo.InvariantCulture)
            );
            clockObj.SetActive(true);
        }

        public static void Update() {
            if (clockObj != null) {
                string format12h = "";
                float minuteFraction = Mathf.Lerp(0f, 24f, EnvMan.instance.GetDayFraction());
                float floor24h = Mathf.Floor(minuteFraction);
                int hours = Mathf.FloorToInt(floor24h);
                int minutes = Mathf.FloorToInt(Mathf.Lerp(0f, 60f, minuteFraction - floor24h));
                if (Main.twelveHourFormat.Value) {
                    format12h = hours < 12 ? "AM" : "PM";
                    if (hours > 12) hours -= 12;
                }
                //Debug.Log($"Clock time : {hours}:{minutes} {format12h}");
                clockObj.GetComponent<Text>().text = $"{(hours < 10 ? "0" : "")}{hours}:{(minutes < 10 ? "0" : "")}{minutes} {format12h}";
            }
        }
    }
}
