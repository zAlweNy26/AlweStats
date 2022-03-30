using System.Collections.Generic;
using UnityEngine;

namespace AlweStats {
    public static class BowStats {
        private static Block bowBlock = null;
        private static GuiBar bowCharge = null;

        public static Block Start() {
            if (Main.enableBowStats.Value) {
                bowBlock = new Block(
                    "BowStats",
                    Main.bowStatsColor.Value,
                    Main.bowStatsSize.Value,
                    Main.bowStatsPosition.Value,
                    Main.bowStatsMargin.Value,
                    Main.bowStatsAlign.Value
                );
                bowBlock.SetActive(false);   
            }
            if (Main.customBowCharge.Value) {
                bowCharge = UnityEngine.Object.Instantiate(Hud.instance.m_stealthBar, Hud.instance.m_stealthBar.transform);
                bowCharge.name = "BowChargeBar";
                bowCharge.transform.SetParent(Hud.instance.m_stealthBar.transform.parent);
                bowCharge.gameObject.SetActive(false);
            }
            return bowBlock;
        }

        public static void Update() {
            Player localPlayer = Player.m_localPlayer;
            if (Main.enableBowStats.Value && bowBlock != null && localPlayer) {
                int currentAmmo = 0, totalAmmo = 0;
                List<ItemDrop.ItemData> inventoryItems = localPlayer.GetInventory().GetAllItems();
                ItemDrop.ItemData bow = inventoryItems.Find(i => i.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Bow);
                if (bow != null) {
                    ItemDrop.ItemData ammoItem = localPlayer.m_ammoItem;
                    if (ammoItem == null) ammoItem = localPlayer.GetInventory().GetAmmoItem(bow.m_shared.m_ammoType);
                    if (ammoItem != null) {
                        foreach (ItemDrop.ItemData i in inventoryItems) {
                            var ish = i.m_shared;
                            if ((ish.m_itemType == ItemDrop.ItemData.ItemType.Ammo || ish.m_itemType == ItemDrop.ItemData.ItemType.Consumable)
                                && ish.m_ammoType == bow.m_shared.m_ammoType) {
                                totalAmmo += i.m_stack;
                                if (ish.m_name == ammoItem.m_shared.m_name) currentAmmo += i.m_stack;
                            }
                        }
                        bowBlock.SetActive(totalAmmo != 0);
                        string arrowLocalized = Localization.instance.Localize(ammoItem.m_shared.m_name);
                        bowBlock.SetText($"Bow ammo : {currentAmmo} / {totalAmmo}\nSelected arrows : {arrowLocalized}");
                    } else bowBlock.SetActive(false);
                } else bowBlock.SetActive(false);
            }
            if (Main.customBowCharge.Value && bowCharge != null && localPlayer) {
                float bowPerc = localPlayer.GetAttackDrawPercentage();
                Hud.instance.m_crosshairBow.gameObject.SetActive(false);
                bowCharge.gameObject.SetActive(bowPerc != 0f);
                bowCharge.SetWidth(Hud.instance.m_stealthBar.m_width);
                bowCharge.SetColor(Color.Lerp(new Color(1f, 1f, 1f, 0f), Utilities.StringToColor(Main.bowChargeBarColor.Value), bowPerc));
                bowCharge.SetValue(bowPerc);
            }
        }
    }
}
