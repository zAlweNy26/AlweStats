using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

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
            enableBeehiveStatus, enableBowStats, customBowCharge, daysInWorldsList, twelveHourFormat, enableShipStatus, 
            showResetButton, customShowBiome, enableContainerStatus, enablePieceStatus, enableEntityStats,
            enableMapStats, showCursorCoordinatesInMap, enableRotatingMinimap, showExploredPercentage, enableFireStatus,
            enableBedStatus, enablePortalStatus, enablePlayerStats, showEntityDistance/*, showCustomMinimap*/;
        public static ConfigEntry<int> 
            gameStatsSize, worldStatsSize, worldClockSize, 
            shipStatsSize, bowStatsSize, mapStatsSize, largeMapInfoSize;
        public static ConfigEntry<float> playerMarkerScale;
        public static ConfigEntry<KeyCode> toggleEditMode, reloadPluginKey;
        public static ConfigEntry<string> 
            gameStatsColor, gameStatsAlign, gameStatsPosition, gameStatsMargin, 
            worldStatsColor, worldStatsAlign, worldStatsPosition, worldStatsMargin,
            shipStatsColor, shipStatsAlign, shipStatsPosition, shipStatsMargin,
            bowStatsColor, bowStatsPosition, bowStatsMargin, bowStatsAlign,
            mapStatsColor, mapStatsPosition, mapStatsMargin, mapStatsAlign,
            worldClockColor, worldClockPosition, worldClockMargin, 
            healthFormat, processFormat, bowChargeBarColor, blocksBackgroundColor, tamedBarColor,
            worldCoordinatesFormat, mapCoordinatesFormat, blocksPadding, showCustomPins, showPinsTitles, biggerPins;

        public void Awake() {
            config = Config;
            config.SaveOnConfigSet = true;

            reloadPluginKey = Config.Bind("General", "ReloadPluginKey", KeyCode.F9, "Key to reload the plugin config file, set it to None to disable it");
            toggleEditMode = Config.Bind("General", "EditModeKey", KeyCode.F8, "Key to toggle hud editing mode, set it to None to disable it");

            enableGameStats = Config.Bind("GameStats", "Enable", true, "Toggle the GameStats block");
            enableWorldStats = Config.Bind("WorldStats", "Enable", true, "Toggle the WorldStats block");
            enableWorldClock = Config.Bind("WorldClock", "Enable", true, "Toggle the WorldClock block");
            enableShipStats = Config.Bind("ShipStats", "Enable", true, "Toggle the ShipStats block");
            enableBowStats = Config.Bind("BowStats", "Enable", true, "Toggle the BowStats block");
            enableMapStats = Config.Bind("MapStats", "Enable", true, "Toggle the MapStats block");
            enableEntityStats = Config.Bind("EntityStats", "Enable", true, "Toggle the EntityStats section");
            enableEnvStats = Config.Bind("EnvStats", "Enable", true, "Toggle the EnvStats section");
            enablePlayerStats = Config.Bind("PlayerStats", "Enable", true, "Toggle the PlayerStats section");

            enableRockStatus = Config.Bind("EnvStats", "RockStatus", true, "Toggle the status for rocks");
            enableTreeStatus = Config.Bind("EnvStats", "TreeStatus", true, "Toggle the status for trees");
            enableBushStatus = Config.Bind("EnvStats", "BushStatus", true, "Toggle the growth status for bushes");
            enablePlantStatus = Config.Bind("EnvStats", "PlantStatus", true, "Toggle the growth status for plants");
            enableFermenterStatus = Config.Bind("EnvStats", "FermenterStatus", true, "Toggle status for fermenters");
            enableFireStatus = Config.Bind("EnvStats", "FireStatus", true, "Toggle status for fireplaces");
            enableBeehiveStatus = Config.Bind("EnvStats", "BeehiveStatus", true, "Toggle the status for beehives");
            enableContainerStatus = Config.Bind("EnvStats", "ContainerStatus", true, "Toggle the status for containers");
            enablePieceStatus = Config.Bind("EnvStats", "PieceStatus", true, "Toggle the status for construction pieces");
            showEntityDistance = Config.Bind("EntityStats", "EntityDistance", true, "Toggle the text about the distance from the player and the hovering entity");
            twelveHourFormat = Config.Bind("WorldClock", "TwelveHourFormat", false, "Toggle the clock in the 12h format with AM and PM");
            showResetButton = Config.Bind("General", "ShowResetButton", true, "Toggle a button in the pause menu to reset the AlweStats values");
            customShowBiome = Config.Bind("WorldStats", "CustomShowBiome", true, "Toggle the current biome in the WorldStats block instead of the top-left corner in minimap");
            daysInWorldsList = Config.Bind("WorldStats", "DaysInWorldsList", true, "Toggle days passed counter in the world list panel");
            customBowCharge = Config.Bind("BowStats", "CustomBowCharge", true, "Toggle a custom bow charge bar instead of the vanilla circle that shrinks");
            //showCustomMinimap = Config.Bind("MapStats", "ShowCustomMinimap", true, "Toggle a custom circular minimap instead of the default one");
            enableRotatingMinimap = Config.Bind("MapStats", "RotatingMinimap", true, "Toggle the rotation for the minimap (prettier when custom minimap is enabled)");
            showCursorCoordinatesInMap = Config.Bind("MapStats", "ShowCursorCoordinatesInMap", true, "Toggle the cursor coordinates in the bottom-left corner of the large map");
            showExploredPercentage = Config.Bind("MapStats", "ShowExploredPercentage", true, "Toggle the explored percentage in the top-left corner of the large map");
            enableBedStatus = Config.Bind("MapStats", "BedStatus", true, "Toggle the status that shows the distance from the claimed bed");
            enablePortalStatus = Config.Bind("MapStats", "PortalStatus", true, "Toggle the status that shows the distance from the closer portal");
            enableShipStatus = Config.Bind("MapStats", "ShipStatus", true, "Toggle the status that shows the distance from the closer ship");

            gameStatsColor = Config.Bind("GameStats", "Color", "255, 183, 92, 255", 
                "The color of the text showed\nThe format is : [Red], [Green], [Blue], [Alpha]\nThe range of possible values is from 0 to 255");
            gameStatsSize = Config.Bind("GameStats", "Size", 14,
                "The size of the text showed\nThe range of possible values is from 0 to the amount of your blindness");
            gameStatsAlign = Config.Bind("GameStats", "Align", "LowerLeft",
                "The alignment of the text showed\nPossible values : LowerLeft, LowerCenter, LowerRight, MiddleLeft, MiddleCenter, MiddleRight, UpperLeft, UpperCenter, UpperRight");
            gameStatsPosition = Config.Bind("GameStats", "Position", "0, 0",
                "The position of the text showed\nThe format is : [X], [Y]\nThe possible values are 0 and 1 and all values between");
            gameStatsMargin = Config.Bind("GameStats", "Margin", "0, 0",
                "The margin from its position of the text showed\nThe format is : [X], [Y]\nThe range of possible values is [-(your screen size in pixels), +(your screen size in pixels)]");

            worldStatsColor = Config.Bind("WorldStats", "Color", "255, 183, 92, 255",
                "The color of the text showed\nThe format is : [Red], [Green], [Blue], [Alpha]\nThe range of possible values is from 0 to 255");
            worldStatsSize = Config.Bind("WorldStats", "Size", 14,
                "The size of the text showed\nThe range of possible values is from 0 to the amount of your blindness");
            worldStatsAlign = Config.Bind("WorldStats", "Align", "LowerRight",
                "The alignment of the text showed\nPossible values : LowerLeft, LowerCenter, LowerRight, MiddleLeft, MiddleCenter, MiddleRight, UpperLeft, UpperCenter, UpperRight");
            worldStatsPosition = Config.Bind("WorldStats", "Position", "1, 0",
                "The position of the text showed\nThe format is : [X], [Y]\nThe possible values are 0 and 1 and all values between");
            worldStatsMargin = Config.Bind("WorldStats", "Margin", "0, 0",
                "The margin from its position of the text showed\nThe format is : [X], [Y]\nThe range of possible values is [-(your screen size in pixels), +(your screen size in pixels)]");

            shipStatsColor = Config.Bind("ShipStats", "Color", "255, 183, 92, 255",
                "The color of the text showed\nThe format is : [Red], [Green], [Blue], [Alpha]\nThe range of possible values is from 0 to 255");
            shipStatsSize = Config.Bind("ShipStats", "Size", 14,
                "The size of the text showed\nThe range of possible values is from 0 to the amount of your blindness");
            shipStatsAlign = Config.Bind("ShipStats", "Align", "MiddleRight",
                "The alignment of the text showed\nPossible values : LowerLeft, LowerCenter, LowerRight, MiddleLeft, MiddleCenter, MiddleRight, UpperLeft, UpperCenter, UpperRight");
            shipStatsPosition = Config.Bind("ShipStats", "Position", "1, 0.25",
                "The position of the text showed\nThe format is : [X], [Y]\nThe possible values are 0 and 1 and all values between");
            shipStatsMargin = Config.Bind("ShipStats", "Margin", "0, 0",
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
            bowStatsAlign = Config.Bind("BowStats", "Align", "MiddleLeft",
                "The alignment of the text showed\nPossible values : LowerLeft, LowerCenter, LowerRight, MiddleLeft, MiddleCenter, MiddleRight, UpperLeft, UpperCenter, UpperRight");
            bowStatsPosition = Config.Bind("BowStats", "Position", "0, 0.5",
                "The position of the text showed\nThe format is : [X], [Y]\nThe possible values are 0 and 1 and all values between");
            bowStatsMargin = Config.Bind("BowStats", "Margin", "0, 0",
                "The margin from its position of the text showed\nThe format is : [X], [Y]\nThe range of possible values is [-(your screen size in pixels), +(your screen size in pixels)]");

            mapStatsColor = Config.Bind("MapStats", "Color", "255, 183, 92, 255",
                "The color of the text showed\nThe format is : [Red], [Green], [Blue], [Alpha]\nThe range of possible values is from 0 to 255");
            mapStatsSize = Config.Bind("MapStats", "Size", 14,
                "The size of the text showed\nThe range of possible values is from 0 to the amount of your blindness");
            mapStatsAlign = Config.Bind("MapStats", "Align", "MiddleRight",
                "The alignment of the text showed\nPossible values : LowerLeft, LowerCenter, LowerRight, MiddleLeft, MiddleCenter, MiddleRight, UpperLeft, UpperCenter, UpperRight");
            mapStatsPosition = Config.Bind("MapStats", "Position", "1, 1",
                "The position of the text showed\nThe format is : [X], [Y]\nThe possible values are 0 and 1 and all values between");
            mapStatsMargin = Config.Bind("MapStats", "Margin", "0, 0",
                "The margin from its position of the text showed\nThe format is : [X], [Y]\nThe range of possible values is [-(your screen size in pixels), +(your screen size in pixels)]");

            bowChargeBarColor = Config.Bind("BowStats", "ChargeBarColor", "255, 183, 92, 255",
                "The color of the bow charge bar\nThe format is : [Red], [Green], [Blue], [Alpha]\nThe range of possible values is from 0 to 255");

            blocksPadding = Config.Bind("General", "BlocksPadding", "5, 5, 5, 5",
                "The space between the text and the background for all the blocks\nThe format is : [left], [right], [top], [bottom]\nThe range of possible values is from 0 to a finite and decent value");
            blocksBackgroundColor = Config.Bind("General", "BlocksBackgroundColor", "0, 0, 0, 125",
                "The color of the background color for all the blocks\nThe format is : [Red], [Green], [Blue], [Alpha]\nThe range of possible values is from 0 to 255");

            tamedBarColor = Config.Bind("EntityStats", "TamedBarColor", "0, 0, 255, 255",
                "The color of the background color for all the blocks\nThe format is : [Red], [Green], [Blue], [Alpha]\nThe range of possible values is from 0 to 255");
            largeMapInfoSize = Config.Bind("MapStats", "LargeMapInfoSize", 16,
                "The size of the text that show the cursor coordinates and explored percentage in the large map\nThe range of possible values is from 0 to the amount of your blindness");
            playerMarkerScale = Config.Bind("MapStats", "PlayerMarkerScale", 1.5f,
                "The multiplier for the scale of the player marker icon in both small and large map");
            showCustomPins = Config.Bind("MapStats", "ShowCustomPins", "1, 2, 3, 4, 5, 6", 
                "Toggle specific custom pins, separate them with a comma (,)" +
                "\n0 = disable all custom pins" +
                "\n1 = troll caves pins" +
                "\n2 = crypts pins" +
                "\n3 = fire holes pins" +
                "\n4 = portals pins" +
                "\n5 = ships pins" +
                "\n6 = carts pins");
            showPinsTitles = Config.Bind("MapStats", "ShowPinsTitles", "4", 
                "Toggle the title for specific custom pins, separate them with a comma (,)" +
                "\n0 = disable for all custom pins" +
                "\n1 = troll caves pins" +
                "\n2 = crypts pins" +
                "\n3 = fire holes pins" +
                "\n4 = portals pins" +
                "\n5 = ships pins" +
                "\n6 = carts pins");
            biggerPins = Config.Bind("MapStats", "BiggerPins", "4, 5, 6", 
                "Double or not the size of specific custom pins, separate them with a comma (,)" +
                "\n0 = disable for all custom pins" +
                "\n1 = troll caves pins" +
                "\n2 = crypts pins" +
                "\n3 = fire holes pins" +
                "\n4 = portals pins" +
                "\n5 = ships pins" +
                "\n6 = carts pins");

            healthFormat = Config.Bind("General", "HealthFormat", "{0} / {1} (<color>{2} %</color>)", 
                "The format of the string when showing the health of environment elements, construction pieces and living entities" + 
                "\nThis string is also used for the stamina of mountable creatures" +
                "\n'{0}' stands for the current health value" +
                "\n'{1}' stands for the total health value" +
                "\n'{2}' stands for the health percentage value" +
                "\n'<color>' and '</color>' mean that the text between them will be colored based on the health percentage");

            worldCoordinatesFormat = Config.Bind("General", "WorldCoordinatesFormat", "(x: {0} | y: {1} | z: {2}) Player\n(x: {3} | y: {4} | z: {5}) Focus", 
                "The format of the string when showing the player coordinates" + 
                "\n'{0}' and '{3}' stand for the x values" +
                "\n'{1}' and '{4}' stand for the y values" +
                "\n'{2}' and '{5}' stand for the z values");

            mapCoordinatesFormat = Config.Bind("General", "MapCoordinatesFormat", "Cursor (x: {0} | z: {1})", 
                "The format of the string when showing the map coordinates where the cursor is pointing in the large map" + 
                "\n'{0}' stands for the x value" +
                "\n'{1}' stands for the z value");

            processFormat = Config.Bind("General", "ProcessFormat", "(<color>{0} %</color>)\n{1}", 
                "The format of the string when showing the process status of plants/bushes/fermenters/beehives/fireplaces" + 
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
            private static List<Block> blocks = null;
            
            [HarmonyPostfix]
            [HarmonyPatch(typeof(Hud), "Awake")]
            private static void PatchHudStart() {
                blocks = new();

                if (enableEntityStats.Value) EntityStats.Start();
                if (enableEnvStats.Value && enablePieceStatus.Value) EnvStats.Start();
                if (enableGameStats.Value) blocks.Add(GameStats.Start());
                if (enableWorldStats.Value) blocks.Add(WorldStats.Start());
                if (enableWorldClock.Value) blocks.Add(WorldClock.Start());
                if (enableShipStats.Value) blocks.Add(ShipStats.Start());
                if (enableMapStats.Value) blocks.Add(MapStats.Start()); else MapStats.Start();
                if (enableBowStats.Value) blocks.Add(BowStats.Start()); else BowStats.Start();

                EditingMode.Start(blocks);
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(Hud), "Update")]
            private static void PatchHudUpdate() {
                if (Input.GetKeyDown(reloadPluginKey.Value)) ReloadConfig();  
                if (Input.GetKeyDown(Main.toggleEditMode.Value)) EditingMode.OnPress();
                EditingMode.Update();
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(Hud), "UpdateCrosshair")]
            private static void PatchHudCrosshair() {
                if (Main.enableEnvStats.Value && Main.enablePieceStatus.Value) EnvStats.PatchHoveringPiece();
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(FejdStartup), "Update")]
            private static void PatchMainMenuUpdate() {
                if (Input.GetKeyDown(reloadPluginKey.Value)) ReloadConfig();    
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(Menu), "Update")]
            private static void PatchEscMenuUpdate() {
                if (Input.GetKeyDown(KeyCode.Escape) && showResetButton.Value) EditingMode.ShowButton();
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(ZNetScene), "Update")]
            private static void PatchGame() {
                MapStats.Update();
                BowStats.Update();
                if (enableWorldStats.Value) WorldStats.Update();
                if (enableWorldClock.Value) WorldClock.Update();
                if (enableShipStats.Value) ShipStats.Update();
                if (enableGameStats.Value) GameStats.Update();
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(ZNet), "OnDestroy")]
            static void PatchWorldEnd() {
                if (blocks != null) EditingMode.Destroy(blocks);
                if (enableWorldStats.Value && daysInWorldsList.Value) WorldStats.UpdateWorldsFile();
            }
        }
    }
}
