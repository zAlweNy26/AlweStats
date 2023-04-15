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
            Minimap.instance.m_biomeNameSmall.gameObject.SetActive(!Main.removeMinimapBiome.Value);
            return worldBlock;
        }

        public static void Update() {
            if (worldBlock != null && EnvMan.instance != null && Minimap.instance != null && ZNet.m_world != null) {
                double timePlayed = ZNet.instance.GetTimeSeconds();
                int daysPlayed = (int)Math.Floor(timePlayed / EnvMan.instance.m_dayLengthSec);
                double minutesPlayed = timePlayed / 60;
                double hoursPlayed = minutesPlayed / 60;
                worldBlock.SetText(string.Format(Main.worldStatsFormat.Value, 
                    daysPlayed, 
                    hoursPlayed < 1 ? $"{minutesPlayed:0.##} m" : $"{hoursPlayed:0.##} h",
                    Minimap.instance.m_biomeNameSmall.text,
                    EnvMan.instance.m_currentEnv.m_name,
                    ZNet.m_world.m_seedName
                ));
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FejdStartup), nameof(FejdStartup.Start))]
        private static void PatchMainMenuStart(ref FejdStartup __instance) {
            worldsList = Utilities.GetWorldInfos();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FejdStartup), nameof(FejdStartup.UpdateWorldList))]
        private static void PatchWorldList(ref FejdStartup __instance) {
            if (!Main.daysInWorldsList.Value) return;
            for (int i = 0; i < __instance.m_worlds.Count; i++) {
                World world = __instance.m_worlds[i];
                GameObject worldObj = __instance.m_worldListElements[i];
                Transform days = UnityEngine.Object.Instantiate(worldObj.transform.Find("name"));
                days.name = "days";
                days.SetParent(worldObj.transform);
                if (Main.HasAuga) {
                    Vector2 namePos = worldObj.transform.Find("name").GetComponent<RectTransform>().anchoredPosition;
                    days.GetComponent<RectTransform>().anchoredPosition = new(Math.Abs(namePos.x), 0f);
                } else days.GetComponent<RectTransform>().localPosition = new(355f, -14f, 0f);
                string daysText = Localization.instance.Localize("0 $alwe_days");
                string dbPath = File.Exists(world.GetDBPath(FileHelpers.FileSource.Local)) ? world.GetDBPath(FileHelpers.FileSource.Local) : world.GetDBPath(FileHelpers.FileSource.Legacy);
                if (File.Exists(dbPath)) {
                    using FileStream fs = File.OpenRead(dbPath);
                    using BinaryReader br = new(fs);
                    int worldVersion = br.ReadInt32();
                    double timePlayed = br.ReadDouble();
                    int daysPlayed = (int) Math.Floor(timePlayed / 1200L); 
                    if (worldsList != null) {
                        WorldInfo worldInfo = worldsList.FirstOrDefault(w => w.worldName == world.m_name);
                        if (worldInfo != null) daysPlayed = (int) Math.Floor(worldInfo.timePlayed / worldInfo.dayLength); 
                    }
                    daysText = Localization.instance.Localize($"{daysPlayed} {(daysPlayed == 1 ? "$alwe_day" : "$alwe_days")}");
                }
                days.GetComponent<Text>().text = daysText;
            }
        }
    }
}
