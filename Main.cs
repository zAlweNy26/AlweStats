using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace AlweStats {
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInProcess("Valheim.exe")]
    public class Main : BaseUnityPlugin {
        private readonly Harmony harmony = new("zAlweNy26.AlweStats");
        public static ConfigFile config;
        public static string statsFilePath;
        public static ConfigEntry<bool> 
            enableGameStats, enableWorldStats, enableWorldStatsInSelection, enableWorldClock, enableShipStats, 
            twelveHourFormat, showResetButton, customShowBiome;
        public static ConfigEntry<int> gameStatsSize, worldStatsSize, worldClockSize, shipStatsSize;
        public static ConfigEntry<KeyCode> toggleEditMode;
        public static ConfigEntry<string> 
            gameStatsColor, gameStatsAlign, gameStatsPosition, gameStatsMargin, 
            worldStatsColor, worldStatsAlign, worldStatsPosition, worldStatsMargin,
            shipStatsColor, shipStatsAlign, shipStatsPosition, shipStatsMargin,
            worldClockColor, worldClockPosition, worldClockMargin;

        public void Awake() {
            config = Config;

            toggleEditMode = Config.Bind("General", "EditModeKey", KeyCode.F9, "Key to toggle hud editing mode, set it to None to disable it");

            enableGameStats = Config.Bind("GameStats", "Enable", true, "Whether or not to show fps and ping counters in game");
            enableWorldStats = Config.Bind("WorldStats", "Enable", true, "Whether or not to show days passed and time played counters in game");
            enableWorldStatsInSelection = Config.Bind("WorldStats", "DaysInWorldList", true, "Whether or not to show days passed counter in world selection");
            enableWorldClock = Config.Bind("WorldClock", "Enable", true, "Whether or not to show a clock in game");
            enableShipStats = Config.Bind("ShipStats", "Enable", true, "Whether or not to show wind speed, wind direction and ship health when on board");

            twelveHourFormat = Config.Bind("WorldClock", "TwelveHourFormat", false, "Whether or not to show the clock in the 12h format with AM and PM");
            showResetButton = Config.Bind("General", "ShowResetButton", true, "Whether or not to show a button in the pause menu to reset the AlweStats values");
            customShowBiome = Config.Bind("WorldStats", "CustomShowBiome", true, "Whether or not to show the current biome in the WorldStats block instead of the top-left corner in minimap");

            gameStatsColor = Config.Bind("GameStats", "Color", "255, 183, 92, 255", 
                "The color of the text showed\nThe format is : [Red], [Green], [Blue], [Alpha]\nThe range of possible values is from 0 to 255");
            gameStatsSize = Config.Bind("GameStats", "Size", 14,
                "The size of the text showed\nThe range of possible values is from 0 to the amount of your blindness");
            gameStatsAlign = Config.Bind("GameStats", "Align", "LowerLeft",
                "The alignment of the text showed\nPossible values : LowerLeft, LowerCenter, LowerRight, MiddleLeft, MiddleCenter, MiddleRight, UpperLeft, UpperCenter, UpperRight");
            gameStatsPosition = Config.Bind("GameStats", "Position", "0, 0",
                "The position of the text showed\nThe format is : [X], [Y]\nThe possible values are 0 and 1 and all values between");
            gameStatsMargin = Config.Bind("GameStats", "Margin", "5, 5",
                "The margin from its position of the text showed\nThe format is : [X], [Y]\nThe range of possible values is [-(your screen size in pixels), +(your screen size in pixels)]");

            worldStatsColor = Config.Bind("WorldStats", "Color", "255, 183, 92, 255",
                "The color of the text showed\nThe format is : [Red], [Green], [Blue], [Alpha]\nThe range of possible values is from 0 to 255");
            worldStatsSize = Config.Bind("WorldStats", "Size", 14,
                "The size of the text showed\nThe range of possible values is from 0 to the amount of your blindness");
            worldStatsAlign = Config.Bind("WorldStats", "Align", "LowerRight",
                "The alignment of the text showed\nPossible values : LowerLeft, LowerCenter, LowerRight, MiddleLeft, MiddleCenter, MiddleRight, UpperLeft, UpperCenter, UpperRight");
            worldStatsPosition = Config.Bind("WorldStats", "Position", "1, 0",
                "The position of the text showed\nThe format is : [X], [Y]\nThe possible values are 0 and 1 and all values between");
            worldStatsMargin = Config.Bind("WorldStats", "Margin", "-5, 5",
                "The margin from its position of the text showed\nThe format is : [X], [Y]\nThe range of possible values is [-(your screen size in pixels), +(your screen size in pixels)]");

            shipStatsColor = Config.Bind("ShipStats", "Color", "255, 183, 92, 255",
                "The color of the text showed\nThe format is : [Red], [Green], [Blue], [Alpha]\nThe range of possible values is from 0 to 255");
            shipStatsSize = Config.Bind("ShipStats", "Size", 14,
                "The size of the text showed\nThe range of possible values is from 0 to the amount of your blindness");
            shipStatsAlign = Config.Bind("ShipStats", "Align", "MiddleRight",
                "The alignment of the text showed\nPossible values : LowerLeft, LowerCenter, LowerRight, MiddleLeft, MiddleCenter, MiddleRight, UpperLeft, UpperCenter, UpperRight");
            shipStatsPosition = Config.Bind("ShipStats", "Position", "1, 0.25",
                "The position of the text showed\nThe format is : [X], [Y]\nThe possible values are 0 and 1 and all values between");
            shipStatsMargin = Config.Bind("ShipStats", "Margin", "-5, 0",
                "The margin from its position of the text showed\nThe format is : [X], [Y]\nThe range of possible values is [-(your screen size in pixels), +(your screen size in pixels)]");

            worldClockColor = Config.Bind("WorldClock", "Color", "255, 183, 92, 255", 
                "The color of the text showed\nThe format is : [Red], [Green], [Blue], [Alpha]\nThe range of possible values is from 0 to 255");
            worldClockSize = Config.Bind("WorldClock", "Size", 24, 
                "The size of the text showed\nThe range of possible values is from 0 to the amount of your blindness");
            worldClockPosition = Config.Bind("WorldClock", "Position", "0.5, 1", 
                "The position of the text showed\nThe format is : [X], [Y]\nThe possible values are 0 and 1 and all values between");
            worldClockMargin = Config.Bind("WorldClock", "Margin", "0, 0", 
                "The margin from its position of the text showed\nThe format is : [X], [Y]\nThe range of possible values is [-(your screen size in pixels), +(your screen size in pixels)]");

            Logger.LogInfo($"AlweStats loaded successfully !");

            statsFilePath = Path.Combine(Paths.PluginPath, "Alwe.stats");
        }

        public void Start() { harmony.PatchAll(); }
        
        public void OnDestroy() { harmony.UnpatchSelf(); }

        [HarmonyPatch]
        public static class PluginStartup {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(Hud), "Awake")]
            private static void PatchHudStart() {
                List<Block> blocks = new();
                if (enableGameStats.Value) blocks.Add(GameStats.Start());
                if (enableWorldStats.Value) blocks.Add(WorldStats.Start());
                if (enableWorldClock.Value) blocks.Add(WorldClock.Start());
                if (enableShipStats.Value) blocks.Add(ShipStats.Start());
                EditingMode.Start(blocks);
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(Hud), "Update")]
            private static void PatchHudUpdate() {
                EditingMode.Update();
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(Menu), "Update")]
            private static void PatchEscMenuUpdate() {
                if (Input.GetKeyDown(KeyCode.Escape) && showResetButton.Value) EditingMode.ShowButton();
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(ZNetScene), "Update")]
            private static void PatchGame() {
                if (enableGameStats.Value) GameStats.Update();
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(EnvMan), "FixedUpdate")]
            private static void PatchWorld() {
                if (enableWorldStats.Value) WorldStats.Update();
                if (enableWorldClock.Value) WorldClock.Update();
                if (enableShipStats.Value) ShipStats.Update();
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(Ship), "OnTriggerEnter")]
            private static void PatchShipEnter() {
                if (enableShipStats.Value) ShipStats.Show();
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(Ship), "OnTriggerExit")]
            private static void PatchShipExit() {
                if (enableShipStats.Value) ShipStats.Hide();
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(FejdStartup), "ShowStartGame")]
            static void PatchWorldList(ref FejdStartup __instance) {
                if (enableWorldStatsInSelection.Value) WorldStats.UpdateWorldsPanel();
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(FejdStartup), "OnSelectWorld")]
            static void PatchWorldSelection(ref FejdStartup __instance) {
                if (enableWorldStatsInSelection.Value) WorldStats.UpdateWorldsPanel();
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(FejdStartup), "OnButtonRemoveWorldYes")]
            static void PatchWorldRemove(ref FejdStartup __instance) {
                if (enableWorldStatsInSelection.Value) WorldStats.UpdateWorldsPanel();
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(ZNet), "OnDestroy")]
            static void PatchWorldEnd(ref ZNet __instance) {
                if (enableWorldStatsInSelection.Value) WorldStats.UpdateWorldsFile();
                EditingMode.Destroy();
            }
        }
    }
}
