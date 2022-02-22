using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace AlweStats {
    public static class WorldStats {
        private static GameObject worldObj = null;
        private static string[] statsFileLines;

        public static void Start() {
            worldObj = new GameObject("WorldStats");
            worldObj.transform.SetParent(Hud.instance.transform.Find("hudroot"));
            worldObj.AddComponent<RectTransform>();
            ContentSizeFitter contentFitter = worldObj.AddComponent<ContentSizeFitter>();
            contentFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            Text worldText = worldObj.AddComponent<Text>();
            string[] colors = Regex.Replace(Main.worldStatsColor.Value, @"\s+", "").Split(',');
            worldText.color = new(
                Mathf.Clamp01(float.Parse(colors[0], CultureInfo.InvariantCulture) / 255f),
                Mathf.Clamp01(float.Parse(colors[1], CultureInfo.InvariantCulture) / 255f),
                Mathf.Clamp01(float.Parse(colors[2], CultureInfo.InvariantCulture) / 255f),
                Mathf.Clamp01(float.Parse(colors[3], CultureInfo.InvariantCulture) / 255f)
            );
            worldText.font = MessageHud.instance.m_messageCenterText.font;
            worldText.fontSize = Main.worldStatsSize.Value;
            worldText.enabled = true;
            Enum.TryParse(Main.worldStatsAlign.Value, out TextAnchor textAlignment);
            worldText.alignment = textAlignment;
            RectTransform statsRect = worldObj.GetComponent<RectTransform>();
            string[] positions = Regex.Replace(Main.worldStatsPosition.Value, @"\s+", "").Split(',');
            statsRect.anchorMax = statsRect.anchorMin = statsRect.pivot = new(
                float.Parse(positions[0], CultureInfo.InvariantCulture),
                float.Parse(positions[1], CultureInfo.InvariantCulture)
            );
            string[] margins = Regex.Replace(Main.worldStatsMargin.Value, @"\s+", "").Split(',');
            statsRect.anchoredPosition = new(
                float.Parse(margins[0], CultureInfo.InvariantCulture),
                float.Parse(margins[1], CultureInfo.InvariantCulture)
            );
            worldObj.SetActive(true);
        }

        public static void Update() {
            if (worldObj != null) {
                double timePlayed = ZNet.instance.GetTimeSeconds();
                int daysPlayed = (int) Math.Floor(timePlayed / EnvMan.instance.m_dayLengthSec);
                double minutesPlayed = timePlayed / 60;
                double hoursPlayed = minutesPlayed / 60;
                //string currentBiome = string.Join(" ", Regex.Split(Enum.GetName(typeof(Heightmap.Biome), EnvMan.instance.GetCurrentBiome()), @"(?<!^)(?=[A-Z])"));
                //if (hoursPlayed < 1) worldObj.GetComponent<Text>().text = $"Days passed : {daysPlayed}\nTime played : {minutesPlayed:0.00} m\nCurrent biome : {currentBiome}";
                //else worldObj.GetComponent<Text>().text = $"Days passed : {daysPlayed}\nTime played : {hoursPlayed:0.00} h\nCurrent biome : {currentBiome}";
                if (hoursPlayed < 1) worldObj.GetComponent<Text>().text = $"Days passed : {daysPlayed}\nTime played : {minutesPlayed:0.00} m";
                else worldObj.GetComponent<Text>().text = $"Days passed : {daysPlayed}\nTime played : {hoursPlayed:0.00} h";
                //Debug.Log($"Days : {days} | Hours played : {hoursPlayed:0.00} | Current biome : {currentBiome}");
            }
        }

        public static void UpdateWorldsPanel() {
            List<string> worlds = new();
            if (File.Exists(Main.statsFilePath)) statsFileLines = File.ReadAllLines(Main.statsFilePath);
            foreach (Transform t in FejdStartup.instance.m_worldListRoot) {
                Transform days = UnityEngine.Object.Instantiate(t.Find("name"));
                days.name = "days";
                days.SetParent(t.Find("name").parent);
                days.GetComponent<RectTransform>().localPosition = new(325f, -14f, 0f);
                string worldName = t.Find("name").GetComponent<Text>().text;
                string dBPath = $"{Utils.GetSaveDataPath()}/worlds/{worldName}.db";
                if (File.Exists(dBPath)) {
                    using FileStream fs = File.OpenRead(dBPath);
                    using BinaryReader br = new(fs);
                    int worldVersion = br.ReadInt32();
                    if (worldVersion >= 4) {
                        double timePlayed = br.ReadDouble();
                        long daySeconds = 1200L;
                        if (statsFileLines != null) {
                            string str = statsFileLines.FirstOrDefault(s => s.Contains(worldName));
                            if (str != null) daySeconds = long.Parse(statsFileLines[Array.IndexOf(statsFileLines, str)].Split(':')[1]);
                        }
                        int daysPlayed = (int) Math.Floor(timePlayed / daySeconds);
                        //Debug.Log($"{worldName} : {timePlayed} seconds | {daysPlayed} days");
                        days.GetComponent<Text>().text = $"{daysPlayed} days";
                        if (!worlds.Contains(worldName)) worlds.Add(worldName);
                    }
                } else days.GetComponent<Text>().text = "0 days";
                days.gameObject.SetActive(true);
            }
        }

        public static void UpdateWorldsFile() {
            if (!Main.enableWorldStatsInSelection.Value) return;
            string worldName = ZNet.instance.GetWorldName();
            if (File.Exists(Main.statsFilePath)) {
                statsFileLines = File.ReadAllLines(Main.statsFilePath);
                string str = statsFileLines.FirstOrDefault(s => s.Contains(worldName));
                if (str != null) {
                    statsFileLines[Array.IndexOf(statsFileLines, str)] = $"{worldName}:{EnvMan.instance.m_dayLengthSec}";
                    File.WriteAllLines(Main.statsFilePath, statsFileLines);
                } else File.AppendAllText(Main.statsFilePath, $"{Environment.NewLine}{worldName}:{EnvMan.instance.m_dayLengthSec}");
            } else File.AppendAllText(Main.statsFilePath, $"{worldName}:{EnvMan.instance.m_dayLengthSec}");
        }
    }
}
