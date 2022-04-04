using System;
using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.Events;

namespace AlweStats {
    [HarmonyPatch]
    public static class WorldStats {
        private static Block worldBlock = null;
        private static List<WorldInfo> worldsList;

        public static Block Start() {
            worldBlock = new Block(
                "WorldStats",
                Main.worldStatsColor.Value,
                Main.worldStatsSize.Value,
                Main.worldStatsPosition.Value,
                Main.worldStatsMargin.Value,
                Main.worldStatsAlign.Value
            );
            return worldBlock;
        }

        public static void Update() {
            if (worldBlock != null) {
                double timePlayed = ZNet.instance.GetTimeSeconds();
                int daysPlayed = (int)Math.Floor(timePlayed / EnvMan.instance.m_dayLengthSec);
                double minutesPlayed = timePlayed / 60;
                double hoursPlayed = minutesPlayed / 60;
                string currentBiome = "";
                if (Main.customShowBiome.Value) {
                    currentBiome = $"\nBiome : {Minimap.instance.m_biomeNameSmall.text}";
                    Minimap.instance.m_biomeNameSmall.gameObject.SetActive(false);
                }
                if (hoursPlayed < 1) worldBlock.SetText($"Days : {daysPlayed}\nPlay time : {minutesPlayed:0.##} m{currentBiome}");
                else worldBlock.SetText($"Days : {daysPlayed}\nPlay time : {hoursPlayed:0.##} h{currentBiome}");
                //Debug.Log($"Days : {days} | Hours played : {hoursPlayed:0.##} | Current biome : {currentBiome}");
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FejdStartup), "Start")]
        private static void PatchMainMenuStart() {
            worldsList = Utilities.UpdateWorldFile();
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(FejdStartup), "UpdateWorldList")]
        static bool PatchWorldList(FejdStartup __instance, bool centerSelection) {
            if (!Main.daysInWorldsList.Value) return true;
            __instance.m_worlds = World.GetWorldList();
            foreach (GameObject obj in __instance.m_worldListElements) UnityEngine.Object.Destroy(obj);
            __instance.m_worldListElements.Clear();
            float num = (float) __instance.m_worlds.Count * __instance.m_worldListElementStep;
            num = Mathf.Max(__instance.m_worldListBaseSize, num);
            __instance.m_worldListRoot.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, num);
            for (int i = 0; i < __instance.m_worlds.Count; i++) {
                World world = __instance.m_worlds[i];
                GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(__instance.m_worldListElement, __instance.m_worldListRoot);
                gameObject.SetActive(true);
                (gameObject.transform as RectTransform).anchoredPosition = new Vector2(0f, (float)i * -__instance.m_worldListElementStep);
                gameObject.GetComponent<Button>().onClick.AddListener(new UnityAction(__instance.OnSelectWorld));
                Text component = gameObject.transform.Find("seed").GetComponent<Text>();
                component.text = "Seed:" + world.m_seedName;
                gameObject.transform.Find("name").GetComponent<Text>().text = world.m_name;
                Transform days = UnityEngine.Object.Instantiate(gameObject.transform.Find("name"));
                days.name = "days";
                days.SetParent(gameObject.transform);
                days.GetComponent<RectTransform>().localPosition = new(320f, -14f, 0f);
                string daysText = "0 days";
                if (File.Exists(world.GetDBPath())) {
                    using FileStream fs = File.OpenRead(world.GetDBPath());
                    using BinaryReader br = new(fs);
                    int worldVersion = br.ReadInt32();
                    double timePlayed = br.ReadDouble();
                    int daysPlayed = (int) Math.Floor(timePlayed / 1200L); 
                    if (worldsList != null) {
                        WorldInfo worldInfo = worldsList.FirstOrDefault(w => w.worldName == world.m_name);
                        if (worldInfo != null) daysPlayed = (int) Math.Floor(worldInfo.timePlayed / worldInfo.dayLength); 
                    }
                    daysText = $"{daysPlayed} {(daysPlayed == 1 ? "day" : "days")}";
                }
                days.GetComponent<Text>().text = daysText;
                if (world.m_loadError) component.text = " [LOAD ERROR]";
                else if (world.m_versionError) component.text = " [BAD VERSION]";
                RectTransform rectTransform = gameObject.transform.Find("selected") as RectTransform;
                bool flag = __instance.m_world != null && world.m_name == __instance.m_world.m_name;
                rectTransform.gameObject.SetActive(flag);
                if (flag && centerSelection) __instance.m_worldListEnsureVisible.CenterOnItem(rectTransform);
                __instance.m_worldListElements.Add(gameObject);
            }
            return false;
        }

        /*private static void UpdateWorldsPanel() {
            List<string> worlds = new();
            if (File.Exists(Main.statsFilePath)) statsFileLines = File.ReadAllLines(Main.statsFilePath);
            foreach (Transform t in FejdStartup.instance.m_worldListRoot) {
                Transform days = UnityEngine.Object.Instantiate(t.Find("name"));
                days.name = "days";
                days.SetParent(t.Find("name").parent);
                days.GetComponent<RectTransform>().localPosition = new(320f, -14f, 0f);
                string worldName = t.Find("name").GetComponent<Text>().text;
                string dBPath = $"{Utils.GetSaveDataPath()}/worlds/{worldName}.db";
                if (File.Exists(dBPath)) {
                    using FileStream fs = File.OpenRead(dBPath);
                    using BinaryReader br = new(fsdouble timePlayed = br.ReadDouble(););
                    int worldVersion = br.ReadInt32();
                    if (worldVersion >= 4) {
                        
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

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FejdStartup), "ShowStartGame")]
        static void PatchWorldList(FejdStartup __instance) {
            if (Main.daysInWorldsList.Value) UpdateWorldsPanel();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FejdStartup), "SetSelectedWorld")]
        static void PatchWorldSelection(FejdStartup __instance) {
            if (Main.daysInWorldsList.Value) UpdateWorldsPanel();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FejdStartup), "OnSelectWorldTab")]
        static void PatchWorldSelectionTab(FejdStartup __instance) {
            if (Main.daysInWorldsList.Value) UpdateWorldsPanel();
        }*/
    }
}
