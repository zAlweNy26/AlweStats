using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BepInEx.Bootstrap;
using BepInEx.Configuration;

namespace AlweStats {
    [HarmonyPatch]
    public static class PlayerStats {
        private static Block playerBlock = null;
        private static GuiBar bowCharge = null;
        private static GameObject weightObj = null;
        private static int modSlots = 0;
        public static bool isMinimalEffect = false;
        private static float entrySpacingEffects = 0.0f;

        public static Block Start() {
            if (Chainloader.PluginInfos.ContainsKey("randyknapp.mods.equipmentandquickslots")) {
                Chainloader.PluginInfos.TryGetValue("randyknapp.mods.equipmentandquickslots", out BepInEx.PluginInfo modInfo);
                ConfigFile modConfig = modInfo.Instance.Config;
                modConfig.TryGetEntry(new ("Toggles", "Enable Equipment Slots"), out ConfigEntry<bool> equipSlots);
                modConfig.TryGetEntry(new ("Toggles", "Enable Quick Slots"), out ConfigEntry<bool> quickSlots);
                int moreSlots = (equipSlots.Value ? 5 : 0) + (quickSlots.Value ? 3 : 0);
                modSlots += moreSlots;
                Debug.Log($"Found randyknapp's \"Equipment and quick slots\" mod, so added {moreSlots} more slots");
            }
            if (Chainloader.PluginInfos.ContainsKey("aedenthorn.ExtendedPlayerInventory")) {
                Chainloader.PluginInfos.TryGetValue("aedenthorn.ExtendedPlayerInventory", out BepInEx.PluginInfo modInfo);
                ConfigFile modConfig = modInfo.Instance.Config;
                modConfig.TryGetEntry(new ("Toggles", "ExtraRows"), out ConfigEntry<int> extraRows);
                modConfig.TryGetEntry(new ("Toggles", "AddEquipmentRow"), out ConfigEntry<bool> equipSlots);
                int moreSlots = (extraRows.Value * 8) + (equipSlots.Value ? 8 : 0);
                modSlots += moreSlots;
                Debug.Log($"Found aedenthorn's \"Extended player inventory\" mod, so added {moreSlots} more slots");
            }
            if (Main.enablePlayerStats.Value) {
                playerBlock = new Block(
                    "PlayerStats",
                    Main.playerStatsColor.Value,
                    Main.playerStatsSize.Value,
                    Main.playerStatsPosition.Value,
                    Main.playerStatsMargin.Value,
                    Main.playerStatsAlign.Value
                ); 
            }
            if (Main.customBowCharge.Value) {
                Hud.instance.m_crosshairBow.GetComponentInChildren<Image>().enabled = false;
                bowCharge = UnityEngine.Object.Instantiate(Hud.instance.m_stealthBar, Hud.instance.m_stealthBar.transform.parent);
                bowCharge.name = "BowChargeBar";
                bowCharge.gameObject.SetActive(false);
            }
            if (Main.enableWeightStatus.Value) {
                GameObject template = Hud.instance.m_statusEffectTemplate.gameObject;
                weightObj = UnityEngine.Object.Instantiate(template, template.transform.parent);
                weightObj.name = "WeightStatus";
                weightObj.GetComponentInChildren<Image>().sprite = Minimap.instance.GetLocationIcon("Vendor_BlackForest");
                weightObj.transform.Find("Name").GetComponent<Text>().text = Localization.instance.Localize("$item_weight");
                weightObj.transform.Find("TimeText").GetComponent<Text>().text = "0 %";
                weightObj.SetActive(true);
            }
            if (Chainloader.PluginInfos.ContainsKey("randyknapp.mods.minimalstatuseffects") && Main.enableWeightStatus.Value) {
                isMinimalEffect = true;
                Chainloader.PluginInfos.TryGetValue("randyknapp.mods.minimalstatuseffects", out BepInEx.PluginInfo modInfo);
                ConfigFile modConfig = modInfo.Instance.Config;
                modConfig.TryGetEntry(new ("General", "ListSize"), out ConfigEntry<Vector2> listSize);
                modConfig.TryGetEntry(new ("General", "EntrySpacing"), out ConfigEntry<float> entrySpacing);
                entrySpacingEffects = entrySpacing.Value;
                modConfig.TryGetEntry(new ("General", "IconSize"), out ConfigEntry<float> iconSize);
                modConfig.TryGetEntry(new ("General", "FontSize"), out ConfigEntry<int> fontSize);
                RectTransform nameRect = weightObj.transform.Find("Name") as RectTransform;
                Text nameText = nameRect.GetComponent<Text>();
                weightObj.transform.Find("TimeText").gameObject.SetActive(false);
                nameText.alignment = TextAnchor.MiddleLeft;
                nameText.supportRichText = true;
                nameText.horizontalOverflow = HorizontalWrapMode.Wrap;
                nameText.resizeTextForBestFit = false;
                nameText.fontSize = fontSize.Value;
                nameRect.anchorMin = new Vector2(0, 0.5f);
                nameRect.anchorMax = new Vector2(1, 0.5f);
                nameRect.anchoredPosition = new (120 + iconSize.Value, 2);
                nameRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, iconSize.Value + 20);
                nameRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, listSize.Value.x);
                RectTransform iconRect = weightObj.transform.Find("Icon") as RectTransform;
                iconRect.anchorMin = new Vector2(0.5f, 0.5f);
                iconRect.anchorMax = new Vector2(0.5f, 0.5f);
                iconRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, iconSize.Value);
                iconRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, iconSize.Value);
                iconRect.anchoredPosition = new Vector2(iconSize.Value, 0);
            }
            float crosshairSize = Utilities.GetCultureInvariant<float>(Main.crosshairScale.Value);
            RectTransform crosshairRect = Hud.instance.m_crosshair.gameObject.GetComponent<RectTransform>();
            crosshairRect.sizeDelta = crosshairRect.sizeDelta * new Vector2(crosshairSize, crosshairSize);
            return playerBlock;
        }

        public static void Update() {
            Player localPlayer = Player.m_localPlayer;
            if (Main.enablePlayerStats.Value && playerBlock != null && localPlayer) {
                string arrowLocalized = "None";
                Inventory inventory = localPlayer.GetInventory();
                List<ItemDrop.ItemData> inventoryItems = inventory.GetAllItems();
                float weight = inventory.GetTotalWeight(), totalWeight = Player.m_localPlayer.GetMaxCarryWeight();
                int slots = inventoryItems.Count, totalSlots = inventory.GetWidth() * inventory.GetHeight(), ammo = 0, totalAmmo = 0;
                ItemDrop.ItemData bow = inventoryItems.Find(i => i.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Bow);
                if (bow != null) {
                    ItemDrop.ItemData ammoItem = localPlayer.m_ammoItem;
                    if (ammoItem == null) ammoItem = inventory.GetAmmoItem(bow.m_shared.m_ammoType);
                    if (ammoItem != null) {
                        foreach (ItemDrop.ItemData i in inventoryItems) {
                            var ish = i.m_shared;
                            if ((ish.m_itemType == ItemDrop.ItemData.ItemType.Ammo || ish.m_itemType == ItemDrop.ItemData.ItemType.Consumable)
                                && ish.m_ammoType == bow.m_shared.m_ammoType) {
                                totalAmmo += i.m_stack;
                                if (ish.m_name == ammoItem.m_shared.m_name) ammo += i.m_stack;
                            }
                        }
                        arrowLocalized = Localization.instance.Localize(ammoItem.m_shared.m_name);
                    }
                }
                playerBlock.SetText(string.Format(Main.playerStatsFormat.Value, slots, totalSlots + modSlots, weight, totalWeight, ammo, totalAmmo, arrowLocalized));
            }
            if (Main.customBowCharge.Value && bowCharge != null && localPlayer) {
                float bowPerc = localPlayer.GetAttackDrawPercentage();
                bowCharge.gameObject.SetActive(bowPerc != 0f);
                bowCharge.SetWidth(Hud.instance.m_stealthBar.m_width);
                bowCharge.SetColor(Color.Lerp(new Color(1f, 1f, 1f, 0f), Utilities.StringToColor(Main.bowChargeBarColor.Value), bowPerc));
                bowCharge.SetValue(bowPerc);
            }
            if (Main.enableWeightStatus.Value && weightObj != null && localPlayer) {
                int totEffects = Hud.instance.m_statusEffects.Count;
                Text weightText = weightObj.transform.Find("TimeText").GetComponent<Text>();
                Inventory inventory = localPlayer.GetInventory();
                float weight = inventory.GetTotalWeight(), totalWeight = Player.m_localPlayer.GetMaxCarryWeight();
                float weightPerc = weight / totalWeight * 100f;
                Color currentColor = Color.Lerp(new Color(0f, 1f, 0f, 1f), new Color(1f, 0f, 0f, 1f), weightPerc / 100f);
                if (isMinimalEffect) {
                    weightObj.GetComponent<RectTransform>().localPosition = new Vector3(0, - totEffects * (entrySpacingEffects - 1), 0);
                    Text nameText = weightObj.transform.Find("Name").GetComponent<Text>();
                    string hexColor = Mathf.RoundToInt(currentColor.r * 255f).ToString("X2") + 
                        Mathf.RoundToInt(currentColor.g * 255f).ToString("X2") +
                        Mathf.RoundToInt(currentColor.b * 255f).ToString("X2");
                    nameText.text = Localization.instance.Localize($"$item_weight <color=#{hexColor}>{weightPerc:0.#} %</color>");
                } else {
                    weightObj.GetComponent<RectTransform>().anchoredPosition = new Vector3(-4f - totEffects * Hud.instance.m_statusEffectSpacing, 0f);
                    weightText.text = $"{weightPerc:0.#} %";
                    weightText.color = currentColor;
                }
            }
        }

        public static void PatchCrosshairColor() {
            Hud hud = Hud.instance;
            if (hud.m_hoverName != null) hud.m_crosshair.color = hud.m_hoverName.text.Length > 0 ? Color.yellow : Utilities.StringToColor(Main.crosshairColor.Value);
            else hud.m_crosshair.color = Utilities.StringToColor(Main.crosshairColor.Value);
        }

        private static void PatchCharacterSelection(FejdStartup instance) {
            if (instance.m_profiles == null) instance.m_profiles = SaveSystem.GetAllPlayerProfiles();
            if (instance.m_profileIndex >= instance.m_profiles.Count) instance.m_profileIndex = instance.m_profiles.Count - 1;
            if (instance.m_profileIndex >= 0 && instance.m_profileIndex < instance.m_profiles.Count) {
                PlayerProfile playerProfile = instance.m_profiles[instance.m_profileIndex];
                PlayerProfile.PlayerStats playerStats = playerProfile.m_playerStats;
                instance.m_csName.text = $"{playerProfile.GetName()}\n" +
                    Localization.instance.Localize($"<size=20>$alwe_kills: {playerStats.m_kills}   $alwe_deaths: {playerStats.m_deaths}   $alwe_crafts: {playerStats.m_crafts}   $alwe_builds: {playerStats.m_builds}</size>");
                instance.m_csName.gameObject.SetActive(true);
                instance.SetupCharacterPreview(playerProfile);
                return;
            }
            instance.m_csName.gameObject.SetActive(false);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FejdStartup), "Awake")]
        static void PatchFejdAwake(ref FejdStartup __instance) {
            if (!Main.enablePlayerInfos.Value) return;
            Vector3 sourcePos = __instance.m_csFileSource.transform.position;
            __instance.m_csFileSource.transform.position = new Vector3(sourcePos.x, sourcePos.y + 100f, sourcePos.z);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FejdStartup), "ShowCharacterSelection")]
        static void PatchCharacterSelectionShow(ref FejdStartup __instance) {
            if (Main.enablePlayerInfos.Value) PatchCharacterSelection(__instance);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FejdStartup), "UpdateCharacterList")]
        static void PatchCharacterSelectionUpdate(ref FejdStartup __instance) {
            if (Main.enablePlayerInfos.Value) PatchCharacterSelection(__instance);
        }
    }
}
