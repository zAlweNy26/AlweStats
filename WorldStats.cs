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
            if (worldBlock != null) {
                double timePlayed = ZNet.instance.GetTimeSeconds();
                int daysPlayed = (int)Math.Floor(timePlayed / EnvMan.instance.m_dayLengthSec);
                double minutesPlayed = timePlayed / 60;
                double hoursPlayed = minutesPlayed / 60;
                int worldSeed = ZNet.m_world.m_seed;
                worldBlock.SetText(string.Format(Main.worldStatsFormat.Value, 
                    daysPlayed, 
                    hoursPlayed < 1 ? $"{minutesPlayed:0.##} m" : $"{hoursPlayed:0.##} h",
                    Minimap.instance.m_biomeNameSmall.text,
                    EnvMan.instance.m_currentEnv.m_name,
                    worldSeed
                ));
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FejdStartup), nameof(FejdStartup.Start))]
        private static void PatchMainMenuStart() {
            worldsList = Utilities.GetWorldInfos();
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(FejdStartup), nameof(FejdStartup.UpdateWorldList))]
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
                /*if (Main.HasAuga) {
                    Vector2 namePos = gameObject.transform.Find("name").GetComponent<RectTransform>().anchoredPosition;
                    days.GetComponent<RectTransform>().anchoredPosition = new(Math.Abs(namePos.x), 0f);
                } else*/ days.GetComponent<RectTransform>().localPosition = new(355f, -14f, 0f);
                string daysText = "0 days";
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
    }
}
