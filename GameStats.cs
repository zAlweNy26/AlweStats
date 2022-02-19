using System;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace AlweStats {
    public static class GameStats {
        private static GameObject gameObj = null;
        private static float frameTimer;
        private static int frameSamples;

        public static void Start() {
            gameObj = new GameObject("GameStats");
            gameObj.transform.SetParent(Hud.instance.transform.Find("hudroot"));
            gameObj.AddComponent<RectTransform>();
            ContentSizeFitter contentFitter = gameObj.AddComponent<ContentSizeFitter>();
            contentFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            Text gameText = gameObj.AddComponent<Text>();
            string[] colors = Regex.Replace(Main.gameStatsColor.Value, @"\s+", "").Split(',');
            gameText.color = new(
                Mathf.Clamp01(float.Parse(colors[0], CultureInfo.InvariantCulture) / 255f),
                Mathf.Clamp01(float.Parse(colors[1], CultureInfo.InvariantCulture) / 255f),
                Mathf.Clamp01(float.Parse(colors[2], CultureInfo.InvariantCulture) / 255f),
                Mathf.Clamp01(float.Parse(colors[3], CultureInfo.InvariantCulture) / 255f)
            );
            gameText.font = MessageHud.instance.m_messageCenterText.font;
            gameText.fontSize = Main.gameStatsSize.Value;
            gameText.enabled = true;
            Enum.TryParse(Main.gameStatsAlign.Value, out TextAnchor textAlignment);
            gameText.alignment = textAlignment;
            RectTransform statsRect = gameObj.GetComponent<RectTransform>();
            string[] positions = Regex.Replace(Main.gameStatsPosition.Value, @"\s+", "").Split(',');
            statsRect.anchorMax = statsRect.anchorMin = statsRect.pivot = new(
                float.Parse(positions[0], CultureInfo.InvariantCulture),
                float.Parse(positions[1], CultureInfo.InvariantCulture)
            );
            string[] margins = Regex.Replace(Main.gameStatsMargin.Value, @"\s+", "").Split(',');
            statsRect.anchoredPosition = new Vector2(
                float.Parse(margins[0], CultureInfo.InvariantCulture),
                float.Parse(margins[1], CultureInfo.InvariantCulture)
            );
            gameObj.SetActive(true);
        }

        public static void Update() {
            if (gameObj != null) {
                string fps = GetFPS();
                ZNet.instance.GetNetStats(out var localQuality, out var remoteQuality, out var ping, out var outByteSec, out var inByteSec);
                if (fps != "0") {
                    //Debug.Log($"FPS : {fps} | Ping : {ping:0} ms");
                    if (ping != 0) gameObj.GetComponent<Text>().text = $"Ping : {ping:0} ms\nFPS : {fps}";
                    else gameObj.GetComponent<Text>().text = $"FPS : {fps}";
                }
            }
        }

        private static string GetFPS() {
            string fpsCalculated = "0";
            frameTimer += Time.deltaTime;
            frameSamples++;
            if (frameTimer > 1f) {
                fpsCalculated = Math.Round(1f / (frameTimer / frameSamples)).ToString();
                frameSamples = 0;
                frameTimer = 0f;
            }
            return fpsCalculated;
        }
    }
}
