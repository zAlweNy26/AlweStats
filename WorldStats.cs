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
                Text seedComp = gameObject.transform.Find("seed").GetComponent<Text>();
                seedComp.text = Localization.instance.Localize($"$menu_seed: {world.m_seedName}");
                Text nameComp = gameObject.transform.Find("name").GetComponent<Text>();
                if (world.m_name == world.m_fileName) nameComp.text = world.m_name;
                else nameComp.text = world.m_name + " (" + world.m_fileName + ")";
                Transform days = UnityEngine.Object.Instantiate(gameObject.transform.Find("name"));
                days.name = "days";
                days.SetParent(gameObject.transform);
                if (Main.HasAuga) {
                    Vector2 namePos = gameObject.transform.Find("name").GetComponent<RectTransform>().anchoredPosition;
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
                Transform transform = gameObject.transform.Find("source_cloud");
                if (transform != null) transform.gameObject.SetActive(world.m_fileSource == FileHelpers.FileSource.Cloud);
                Transform transform2 = gameObject.transform.Find("source_local");
                if (transform2 != null) transform2.gameObject.SetActive(world.m_fileSource == FileHelpers.FileSource.Local);
                Transform transform3 = gameObject.transform.Find("source_legacy");
                if (transform3 != null) transform3.gameObject.SetActive(world.m_fileSource == FileHelpers.FileSource.Legacy);
                if (world.m_loadError) seedComp.text = " [LOAD ERROR]";
                else if (world.m_versionError) seedComp.text = " [BAD VERSION]";
                RectTransform rectTransform = gameObject.transform.Find("selected") as RectTransform;
                bool flag = __instance.m_world != null && world.m_fileName == __instance.m_world.m_fileName;
                rectTransform.gameObject.SetActive(flag);
                if (flag && centerSelection) __instance.m_worldListEnsureVisible.CenterOnItem(rectTransform);
                __instance.m_worldListElements.Add(gameObject);
            }
            __instance.m_worldSourceInfo.text = "";
            __instance.m_worldSourceInfoPanel.SetActive(false);
            if (__instance.m_world != null) {
                __instance.m_worldSourceInfo.text = Localization.instance.Localize((
                    (__instance.m_world.m_fileSource == FileHelpers.FileSource.Legacy) ? "$menu_legacynotice \n\n$menu_legacynotice_worlds \n\n" : ""
                ) + ((!FileHelpers.m_cloudEnabled) ? "$menu_cloudsavesdisabled" : ""));
                __instance.m_worldSourceInfoPanel.SetActive(__instance.m_worldSourceInfo.text.Length > 0);
            }
            return false;
        }
    }
}
