using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AlweStats {
    [HarmonyPatch]
    public static class PlayerStats {
        private static Block playerBlock = null;
        private static GuiBar bowCharge = null;
        private static GameObject weightObj = null;

        public static Block Start() {
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
                bowCharge = UnityEngine.Object.Instantiate(Hud.instance.m_stealthBar, Hud.instance.m_stealthBar.transform.parent);
                bowCharge.name = "BowChargeBar";
                bowCharge.gameObject.SetActive(false);
            }
            if (Main.enableWeightStatus.Value) {
                GameObject template = Hud.instance.m_statusEffectTemplate.gameObject;
                weightObj = UnityEngine.Object.Instantiate(template, template.transform.parent);
                weightObj.name = "WeightStatus";
                weightObj.GetComponentInChildren<Image>().sprite = Minimap.instance.GetLocationIcon("Vendor_BlackForest");
                weightObj.transform.Find("Name").GetComponent<Text>().text = "Weight";
                weightObj.transform.Find("TimeText").GetComponent<Text>().text = "0 %";
                weightObj.SetActive(true);
            }
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
                playerBlock.SetText(string.Format(Main.playerStatsFormat.Value, slots, totalSlots, weight, totalWeight, ammo, totalAmmo, arrowLocalized));
            }
            if (Main.customBowCharge.Value && bowCharge != null && localPlayer) {
                float bowPerc = localPlayer.GetAttackDrawPercentage();
                Hud.instance.m_crosshairBow.gameObject.SetActive(false);
                bowCharge.gameObject.SetActive(bowPerc != 0f);
                bowCharge.SetWidth(Hud.instance.m_stealthBar.m_width);
                bowCharge.SetColor(Color.Lerp(new Color(1f, 1f, 1f, 0f), Utilities.StringToColor(Main.bowChargeBarColor.Value), bowPerc));
                bowCharge.SetValue(bowPerc);
            }
            if (Main.enableWeightStatus.Value && weightObj != null && localPlayer) {
                int totEffects = Hud.instance.m_statusEffects.Count;
                weightObj.GetComponent<RectTransform>().anchoredPosition = new Vector3(-4f - totEffects * Hud.instance.m_statusEffectSpacing, 0f);
                Text weightText = weightObj.transform.Find("TimeText").GetComponent<Text>();
                Inventory inventory = localPlayer.GetInventory();
                float weight = inventory.GetTotalWeight(), totalWeight = Player.m_localPlayer.GetMaxCarryWeight();
                float weightPerc = weight / totalWeight * 100f;
                weightText.text = $"{weightPerc:0.#} %";
                weightText.color = Color.Lerp(new Color(0f, 1f, 0f, 1f), new Color(1f, 0f, 0f, 1f), weightPerc / 100f);
            }
        }

        private static void PatchCharacterSelection(FejdStartup instance) {
            if (instance.m_profiles == null) instance.m_profiles = PlayerProfile.GetAllPlayerProfiles();
            if (instance.m_profileIndex >= instance.m_profiles.Count) instance.m_profileIndex = instance.m_profiles.Count - 1;
            if (instance.m_profileIndex >= 0 && instance.m_profileIndex < instance.m_profiles.Count) {
                PlayerProfile playerProfile = instance.m_profiles[instance.m_profileIndex];
                PlayerProfile.PlayerStats playerStats = playerProfile.m_playerStats;
                instance.m_csName.text = $"{playerProfile.GetName()}\n" +
                    $"<size=20>Kills: {playerStats.m_kills}   Deaths: {playerStats.m_deaths}   Crafts: {playerStats.m_crafts}   Builds: {playerStats.m_builds}</size>";
                instance.m_csName.gameObject.SetActive(true);
                instance.SetupCharacterPreview(playerProfile);
                return;
            }
            instance.m_csName.gameObject.SetActive(false);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FejdStartup), "ShowCharacterSelection")]
        static void PatchCharacterSelectionShow(ref FejdStartup __instance) {
            if (Main.enablePlayerStats.Value) PatchCharacterSelection(__instance);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FejdStartup), "UpdateCharacterList")]
        static void PatchCharacterSelectionUpdate(ref FejdStartup __instance) {
            if (Main.enablePlayerStats.Value) PatchCharacterSelection(__instance);
        }
    }
}
