using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace AlweStats {
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInProcess("Valheim.exe")]
    public partial class Plugin : BaseUnityPlugin {
        private readonly Harmony harmony = new("zAlweNy26.AlweStats");
        public static ConfigEntry<bool> enableGameStats, enableWorldStats;
        public static ConfigEntry<int> gameStatsSize, worldStatsSize;
        public static ConfigEntry<string> 
            gameStatsColor, gameStatsAlign, gameStatsPosition, gameStatsMargin, 
            worldStatsColor, worldStatsAlign, worldStatsPosition, worldStatsMargin;

        void Awake() {
            enableGameStats = Config.Bind("GameStats", "Enable", true, "Whether or not to show fps and ping counters");
            enableWorldStats = Config.Bind("WorldStats", "Enable", true, "Whether or not to show days passed and time played counters");

            gameStatsColor = Config.Bind("GameStats", "Color", "255, 183, 92, 255", 
                "The color of the text showed\nThe format is : [Red], [Green], [Blue], [Alpha]\nThe range of possible values is from 0 to 255");
            gameStatsSize = Config.Bind("GameStats", "Size", 14,
                "The size of the text showed\nThe range of possible values is from 0 to the amount of your blindness");
            gameStatsAlign = Config.Bind("GameStats", "Align", "LowerLeft",
                "The alignment of the text showed\nPossible values : LowerLeft, LowerCenter, LowerRight, MiddleLeft, MiddleCenter, MiddleRight, UpperLeft, UpperCenter, UpperRight");
            gameStatsPosition = Config.Bind("GameStats", "Position", "0, 0",
                "The position of the text showed\nThe format is : [X], [Y]\nThe possible values are 0 and 1 and all values between");
            gameStatsMargin = Config.Bind("GameStats", "Margin", "10, 10",
                "The margin from its position of the text showed\nThe format is : [X], [Y]\nThe range of possible values is [-(your screen size in pixels), +(your screen size in pixels)]");

            worldStatsColor = Config.Bind("WorldStats", "Color", "255, 183, 92, 255",
                "The color of the text showed\nThe format is : [Red], [Green], [Blue], [Alpha]\nThe range of possible values is from 0 to 255");
            worldStatsSize = Config.Bind("WorldStats", "Size", 14,
                "The size of the text showed\nThe range of possible values is from 0 to the amount of your blindness");
            worldStatsAlign = Config.Bind("WorldStats", "Align", "LowerRight",
                "The alignment of the text showed\nPossible values : LowerLeft, LowerCenter, LowerRight, MiddleLeft, MiddleCenter, MiddleRight, UpperLeft, UpperCenter, UpperRight");
            worldStatsPosition = Config.Bind("WorldStats", "Position", "1, 0",
                "The position of the text showed\nThe format is : [X], [Y]\nThe possible values are 0 and 1 and all values between");
            worldStatsMargin = Config.Bind("WorldStats", "Margin", "-10, 10",
                "The margin from its position of the text showed\nThe format is : [X], [Y]\nThe range of possible values is [-(your screen size in pixels), +(your screen size in pixels)]");

            Logger.LogInfo($"Loaded successfully !");

            harmony.PatchAll();
        }

        [HarmonyPatch(typeof(ZNetScene), "Update")]
        static class PatchStats {
            private static GameObject statsObj = null;
            private static float frameTimer;
            private static int frameSamples;

            [HarmonyPostfix]
            static void Postfix() {
                if (!enableGameStats.Value) return;
                string fps = GetFPS();
                ZNet.instance.GetNetStats(out var localQuality, out var remoteQuality, out var ping, out var outByteSec, out var inByteSec);
                if (fps != "0") {
                    //Debug.Log($"FPS : {fps} | Ping : {ping:0} ms");
                    Hud hud = Hud.instance;
                    Text statsText;
                    if (statsObj == null) {
                        MessageHud msgHud = MessageHud.instance;
                        statsObj = new GameObject("GameStats");
                        statsObj.transform.SetParent(hud.m_statusEffectListRoot.transform.parent);
                        statsObj.AddComponent<RectTransform>();
                        statsText = statsObj.AddComponent<Text>();
                        string[] colors = Regex.Replace(gameStatsColor.Value, @"\s+", "").Split(',');
                        statsText.color = new Color(
                            Mathf.Clamp01(float.Parse(colors[0]) / 255f),
                            Mathf.Clamp01(float.Parse(colors[1]) / 255f),
                            Mathf.Clamp01(float.Parse(colors[2]) / 255f),
                            Mathf.Clamp01(float.Parse(colors[3]) / 255f)
                        );
                        statsText.font = msgHud.m_messageCenterText.font;
                        statsText.fontSize = gameStatsSize.Value;
                        statsText.enabled = true;
                        Enum.TryParse(gameStatsAlign.Value, out TextAnchor textAlignment);
                        statsText.alignment = textAlignment;
                        statsText.horizontalOverflow = HorizontalWrapMode.Overflow;
                    } else statsText = statsObj.GetComponent<Text>();

                    if (ping != 0) statsText.text = $"Ping : {ping:0} ms\nFPS : {fps}";
                    else statsText.text = $"FPS : {fps}";

                    RectTransform statsRect = statsObj.GetComponent<RectTransform>();

                    string[] positions = Regex.Replace(gameStatsPosition.Value, @"\s+", "").Split(',');
                    statsRect.anchorMax = statsRect.anchorMin = new Vector2(float.Parse(positions[0]), float.Parse(positions[1]));
                    Vector2 shiftDirection = new Vector2(0.5f - statsRect.anchorMax.x, 0.5f - statsRect.anchorMax.y);

                    string[] margins = Regex.Replace(gameStatsMargin.Value, @"\s+", "").Split(',');
                    statsRect.anchoredPosition = shiftDirection * statsRect.rect.size + new Vector2(float.Parse(margins[0]), float.Parse(margins[1]));

                    statsObj.SetActive(true);
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

        [HarmonyPatch(typeof(EnvMan), "FixedUpdate")]
        static class PatchConnectPanel {
            private static GameObject statsObj = null;
            [HarmonyPrefix]
            static void Prefix(ref EnvMan __instance) {
                if (!enableWorldStats.Value) return;
                double timePlayed = ZNet.instance.GetTimeSeconds();
                int daysPlayed = (int) Math.Floor(timePlayed / __instance.m_dayLengthSec);
                double minutesPlayed = timePlayed / 60;
                double hoursPlayed = minutesPlayed / 60;
                //Debug.Log($"Days : {days} | Hours played : {hoursPlayed:0.00}");
                Hud hud = Hud.instance;
                Text statsText;
                if (statsObj == null) {
                    MessageHud msgHud = MessageHud.instance;
                    statsObj = new GameObject("WorldStats");
                    statsObj.transform.SetParent(hud.m_statusEffectListRoot.transform.parent);
                    statsObj.AddComponent<RectTransform>();
                    statsText = statsObj.AddComponent<Text>();
                    string[] colors = Regex.Replace(worldStatsColor.Value, @"\s+", "").Split(',');
                    statsText.color = new Color(
                        Mathf.Clamp01(float.Parse(colors[0]) / 255f),
                        Mathf.Clamp01(float.Parse(colors[1]) / 255f),
                        Mathf.Clamp01(float.Parse(colors[2]) / 255f),
                        Mathf.Clamp01(float.Parse(colors[3]) / 255f)
                    );
                    statsText.font = msgHud.m_messageCenterText.font;
                    statsText.fontSize = worldStatsSize.Value;
                    statsText.enabled = true;
                    Enum.TryParse(worldStatsAlign.Value, out TextAnchor textAlignment);
                    statsText.alignment = textAlignment;
                    statsText.horizontalOverflow = HorizontalWrapMode.Overflow;
                } else statsText = statsObj.GetComponent<Text>();

                if (hoursPlayed < 1) statsText.text = $"Days passed : {daysPlayed}\nTime played : {minutesPlayed:0.00} m";
                else statsText.text = $"Days passed : {daysPlayed}\nTime played : {hoursPlayed:0.00} h";

                RectTransform statsRect = statsObj.GetComponent<RectTransform>();

                string[] positions = Regex.Replace(worldStatsPosition.Value, @"\s+", "").Split(',');
                statsRect.anchorMax = statsRect.anchorMin = new Vector2(float.Parse(positions[0]), float.Parse(positions[1]));
                Vector2 shiftDirection = new Vector2(0.5f - statsRect.anchorMax.x, 0.5f - statsRect.anchorMax.y);

                string[] margins = Regex.Replace(worldStatsMargin.Value, @"\s+", "").Split(',');
                statsRect.anchoredPosition = shiftDirection * statsRect.rect.size + new Vector2(float.Parse(margins[0]), float.Parse(margins[1]));

                statsObj.SetActive(true);
            }
        }

        /*[HarmonyPatch(typeof(FejdStartup), "UpdateWorldList", new Type[] { typeof(bool) })]
        static class PatchWorldList {
            [HarmonyPostfix]
            static void Postfix(ref FejdStartup __instance) {
                foreach (Transform t in __instance.m_worldListRoot) {
                    Debug.Log($"Nome : {t.name} | Type : {t.GetType()}");
                }
            }
        }*/
    }
}
