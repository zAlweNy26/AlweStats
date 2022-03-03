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
            enableGameStats, enableWorldStats, enableWorldClock, enableShipStats, enableEnvStats, 
            enableRockStatus, enableTreeStatus, enableBushStatus, enablePlantStatus, enableFermenterStatus,
            enableBeehiveStatus, enableBowStats, customBowCharge, daysInWorldsList, twelveHourFormat, 
            showResetButton, customShowBiome, enableContainerStatus;
        public static ConfigEntry<int> gameStatsSize, worldStatsSize, worldClockSize, shipStatsSize, bowStatsSize;
        public static ConfigEntry<KeyCode> toggleEditMode, reloadPluginKey;
        public static ConfigEntry<string> 
            gameStatsColor, gameStatsAlign, gameStatsPosition, gameStatsMargin, 
            worldStatsColor, worldStatsAlign, worldStatsPosition, worldStatsMargin,
            shipStatsColor, shipStatsAlign, shipStatsPosition, shipStatsMargin,
            bowStatsColor, bowStatsPosition, bowStatsMargin, bowChargeBarColor,
            worldClockColor, worldClockPosition, worldClockMargin, 
            healthStringFormat, processStringFormat;

        public void Awake() {
            config = Config;
            config.SaveOnConfigSet = true;

            reloadPluginKey = Config.Bind("General", "ReloadPluginKey", KeyCode.F9, "Key to reload the plugin config file");
            toggleEditMode = Config.Bind("General", "EditModeKey", KeyCode.F8, "Key to toggle hud editing mode, set it to None to disable it");

            enableGameStats = Config.Bind("GameStats", "Enable", true, "Whether or not to show fps and ping counters in game");
            enableWorldStats = Config.Bind("WorldStats", "Enable", true, "Whether or not to show days passed and time played counters in game");
            enableWorldClock = Config.Bind("WorldClock", "Enable", true, "Whether or not to show a clock in game");
            enableShipStats = Config.Bind("ShipStats", "Enable", true, "Whether or not to show wind speed, wind direction and ship health when on board");
            enableEnvStats = Config.Bind("EnvStats", "Enable", true, "Whether or not to show the status about every environment element");
            enableBowStats = Config.Bind("BowStats", "Enable", true, "Whether or not to show the current and total ammo in a text block in the hud");

            enableRockStatus = Config.Bind("EnvStats", "RockStatus", true, "Whether or not to show the status for rocks");
            enableTreeStatus = Config.Bind("EnvStats", "TreeStatus", true, "Whether or not to show the status for trees");
            enableBushStatus = Config.Bind("EnvStats", "BushStatus", true, "Whether or not to show the growth status for bushes");
            enablePlantStatus = Config.Bind("EnvStats", "PlantStatus", true, "Whether or not to show the growth status for plants");
            enableFermenterStatus = Config.Bind("EnvStats", "FermenterStatus", true, "Whether or not to show status for fermenters");
            enableBeehiveStatus = Config.Bind("EnvStats", "BeehiveStatus", true, "Whether or not to show the status for beehives");
            enableContainerStatus = Config.Bind("EnvStats", "ContainerStatus", true, "Whether or not to show the status for containers");
            twelveHourFormat = Config.Bind("WorldClock", "TwelveHourFormat", false, "Whether or not to show the clock in the 12h format with AM and PM");
            showResetButton = Config.Bind("General", "ShowResetButton", true, "Whether or not to show a button in the pause menu to reset the AlweStats values");
            customShowBiome = Config.Bind("WorldStats", "CustomShowBiome", true, "Whether or not to show the current biome in the WorldStats block instead of the top-left corner in minimap");
            daysInWorldsList = Config.Bind("WorldStats", "DaysInWorldsList", true, "Whether or not to show days passed counter in the world list panel");
            customBowCharge = Config.Bind("BowStats", "CustomBowCharge", true, "Whether or not to show a bow charge bar instead of the vanilla circle that shrinks");

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

            bowStatsColor = Config.Bind("BowStats", "Color", "255, 183, 92, 255",
                "The color of the text showed\nThe format is : [Red], [Green], [Blue], [Alpha]\nThe range of possible values is from 0 to 255");
            bowStatsSize = Config.Bind("BowStats", "Size", 14,
                "The size of the text showed\nThe range of possible values is from 0 to the amount of your blindness");
            bowStatsPosition = Config.Bind("BowStats", "Position", "0, 0.5",
                "The position of the text showed\nThe format is : [X], [Y]\nThe possible values are 0 and 1 and all values between");
            bowStatsMargin = Config.Bind("BowStats", "Margin", "5, 0",
                "The margin from its position of the text showed\nThe format is : [X], [Y]\nThe range of possible values is [-(your screen size in pixels), +(your screen size in pixels)]");

            bowChargeBarColor = Config.Bind("BowStats", "ChargeBarColor", "255, 183, 92, 255",
                "The color of the bow charge bar\nThe format is : [Red], [Green], [Blue], [Alpha]\nThe range of possible values is from 0 to 255");

            healthStringFormat = Config.Bind("EnvStats", "HealthStringFormat", "{0} / {1} (<color>{2} %</color>)", 
                "The format of the string when showing the health of the environment element" + 
                "\n'{0}' stands for the current health value" +
                "\n'{1}' stands for the total health value" +
                "\n'{2}' stands for the health Percentage value" +
                "\n'<color>' and '</color>' mean that the text between them will be colored based on the health percentage");

            processStringFormat = Config.Bind("EnvStats", "ProcessStringFormat", "(<color>{0} %</color>)\n{1}", 
                "The format of the string when showing the process status of plants/bushes/fermenters/beehives" + 
                "\n'{0}' stands for the percentage" +
                "\n'{1}' stands for the remaining time" +
                "\n'<color>' and '</color>' mean that the text between them will be colored based on the process percentage");

            Logger.LogInfo($"{PluginInfo.PLUGIN_GUID} loaded successfully !");

            statsFilePath = Path.Combine(Paths.PluginPath, "Alwe.stats");
        }

        public void Start() { harmony.PatchAll(); }
        
        public void OnDestroy() { harmony.UnpatchSelf(); }

        public static void ReloadConfig() { 
            if (File.Exists(config.ConfigFilePath)) {
                config.Reload(); 
                Debug.Log($"The config file of {PluginInfo.PLUGIN_GUID} was reloaded successfully !");
            } else Debug.Log($"The config file of {PluginInfo.PLUGIN_GUID} was not found !");
        }

        [HarmonyPatch]
        public static class PluginPatches {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(Hud), "Awake")]
            private static void PatchHudStart() {
                List<Block> blocks = new();
                if (enableGameStats.Value) blocks.Add(GameStats.Start());
                if (enableWorldStats.Value) blocks.Add(WorldStats.Start());
                if (enableWorldClock.Value) blocks.Add(WorldClock.Start());
                if (enableShipStats.Value) blocks.Add(ShipStats.Start());
                if (enableBowStats.Value) blocks.Add(BowStats.Start());
                else if (customBowCharge.Value) BowStats.Start();
                EditingMode.Start(blocks);
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(Hud), "Update")]
            private static void PatchHudUpdate() {
                if (enableBowStats.Value || customBowCharge.Value) BowStats.Update();
                if (Input.GetKeyDown(reloadPluginKey.Value)) ReloadConfig();    
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

            [HarmonyPrefix]
            [HarmonyPatch(typeof(ZNet), "OnDestroy")]
            static void PatchWorldEnd(ref ZNet __instance) {
                if (daysInWorldsList.Value) WorldStats.UpdateWorldsFile();
                EditingMode.Destroy();
            }
        }
    }
}
