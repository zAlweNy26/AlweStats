using UnityEngine;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System;

namespace AlweStats {
    [HarmonyPatch]
    public static class EnvStats {
        private static readonly List<string> envObjs = new() { "stub", "shrub", "oldlog", "beech", "tree", "rock" };
        private static readonly List<string> treeObjs = new() { "stub", "shrub", "oldlog", "beech", "tree" };
        private static string initialText = "";

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Container), "GetHoverText")]
        public static string PatchContainerHoverText(string __result, Container __instance) {
            if (!Main.enableContainerStatus.Value) return __result;
            if (__instance.m_checkGuardStone && !PrivateArea.CheckAccess(__instance.transform.position, 0f, false, false))
                return Localization.instance.Localize(__instance.m_name + "\n$piece_noaccess");
            int perc = (int) __instance.m_inventory.SlotsUsedPercentage();
            string inventoryString = $"{__instance.m_inventory.NrOfItems()} / {__instance.m_inventory.GetWidth() * __instance.m_inventory.GetHeight()} (<color={GetColor(perc)}>{perc} %</color>)";
            string notEmpty = __instance.m_inventory.NrOfItems() > 0 ? $"\n{inventoryString}" : " ( $piece_container_empty )";
            return Localization.instance.Localize($"{__instance.m_name}{notEmpty}\n[<color=yellow><b>$KEY_Use</b></color>] $piece_container_open");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Destructible), "RPC_Damage")]
        public static void OnDamage(Destructible __instance, HitData hit) {
            if (!Main.enableEnvStats.Value) return;
            List<string> canBeElements = envObjs;
            if (!Main.enableRockStatus.Value) canBeElements.Remove("rock");
            if (!Main.enableTreeStatus.Value) canBeElements = canBeElements.Except(treeObjs).ToList();
            if (canBeElements.Any(e => __instance.gameObject.name.ToLower().Contains(e))) {
                //Debug.Log($"Destructible : {__instance.gameObject.name}");
                ZNetView znv = __instance.m_nview;
                if (znv.IsValid() && hit.GetTotalDamage() > 0f) {
                    float totalHealth = Mathf.RoundToInt(__instance.m_health);
                    float currentHealth = Mathf.RoundToInt(znv.GetZDO().GetFloat("health", __instance.m_health));
                    SetHoverText(__instance.gameObject, currentHealth, totalHealth);
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(TreeBase), "RPC_Damage")]
        public static void OnDamage(TreeBase __instance, HitData hit) {
            if (!Main.enableEnvStats.Value) return;
            if (!Main.enableTreeStatus.Value) return;
            //Debug.Log($"TreeBase : {__instance.gameObject.name}");
            ZNetView znv = __instance.m_nview;
            if (znv.IsValid() && hit.GetTotalDamage() > 0f) {
                float totalHealth = Mathf.RoundToInt(__instance.m_health);
                float currentHealth = Mathf.RoundToInt(znv.GetZDO().GetFloat("health", __instance.m_health));
                SetHoverText(__instance.gameObject, currentHealth, totalHealth);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(TreeLog), "RPC_Damage")]
        public static void OnDamage(TreeLog __instance, HitData hit) {
            if (!Main.enableEnvStats.Value) return;
            if (!Main.enableTreeStatus.Value) return;
            //Debug.Log($"TreeLog : {__instance.gameObject.name}");
            ZNetView znv = __instance.m_nview;
            if (znv.IsValid() && hit.GetTotalDamage() > 0f) {
                float totalHealth = Mathf.RoundToInt(__instance.m_health);
                float currentHealth = Mathf.RoundToInt(znv.GetZDO().GetFloat("health", __instance.m_health));
                SetHoverText(__instance.gameObject, currentHealth, totalHealth);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MineRock), "RPC_Hit")]
        public static void OnDamage(MineRock __instance, HitData hit, int hitAreaIndex) {
            if (!Main.enableEnvStats.Value) return;
            if (!Main.enableRockStatus.Value) return;
            //Debug.Log($"MineRock : {__instance.gameObject.name}");
            ZNetView znv = __instance.m_nview;
            if (znv.IsValid() && hit.GetTotalDamage() > 0f) {
                float initialPieceHealth = Mathf.RoundToInt(__instance.m_health);
                float initialTotalHealth = initialPieceHealth * __instance.m_hitAreas.Length;
                float currentTotalHealth = __instance.m_hitAreas.Select((a, i) => Math.Max(0, znv.GetZDO().GetFloat($"Health{i}", __instance.m_health))).Sum();
                foreach (Transform t in __instance.transform) SetHoverText(t.gameObject, currentTotalHealth, initialTotalHealth);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MineRock5), "RPC_Damage")]
        public static void OnDamage(MineRock5 __instance, HitData hit) {
            if (!Main.enableEnvStats.Value) return;
            if (!Main.enableRockStatus.Value) return;
            //Debug.Log($"MineRock piece : {__instance.gameObject.name}");
            ZNetView znv = __instance.m_nview;
            if (znv.IsValid() && hit.GetTotalDamage() > 0f) {
                float initialPieceHealth = Mathf.RoundToInt(__instance.m_health);
                float initialTotalHealth = initialPieceHealth * __instance.m_hitAreas.Count;
                float currentTotalHealth = __instance.m_hitAreas.Sum(a => Math.Max(0, a.m_health));
                foreach (Transform t in __instance.transform) SetHoverText(t.gameObject, currentTotalHealth, initialTotalHealth);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Plant), "GetHoverText")]
        public static string PatchPlantHoverText(string __result, Plant __instance) {
            if (!Main.enablePlantStatus.Value) return __result;
            if (__instance == null) return __result;
            int growPercentage = Mathf.FloorToInt((float) __instance.TimeSincePlanted() / __instance.GetGrowTime() * 100);
            growPercentage = growPercentage > 100 ? 100 : growPercentage;
            return SetPickableText(growPercentage, __instance.GetHoverName(), (int) __instance.TimeSincePlanted(), (int) __instance.GetGrowTime());
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Pickable), "GetHoverText")]
        public static string PatchPickableHoverText(string __result, Pickable __instance) {
            if (!Main.enableBushStatus.Value || !__instance.name.ToLower().Contains("bush")) return __result;
            DateTime startTime = new DateTime(__instance.m_nview.GetZDO().GetLong("picked_time"));
            float currentGrowTime = (float) (ZNet.instance.GetTime() - startTime).TotalMinutes;
            int growPercentage = Mathf.FloorToInt(currentGrowTime / __instance.m_respawnTimeMinutes * 100);
            growPercentage = growPercentage > 100 ? 100 : growPercentage;
            return SetPickableText(growPercentage, __instance.GetHoverName(), (int) currentGrowTime, __instance.m_respawnTimeMinutes);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Beehive), "GetHoverText")]
        public static string PatchBeehiveHoverText(string __result, Beehive __instance) {
            if (!Main.enableBeehiveStatus.Value) return __result;
            if (!PrivateArea.CheckAccess(__instance.transform.position, 0f, false, false))
                return Localization.instance.Localize(__instance.m_name + "\n$piece_noaccess");
            int honeyLevel = __instance.GetHoneyLevel();
            if (honeyLevel > 0) {
                float current = __instance.GetTimeSinceLastUpdate() + __instance.m_nview.GetZDO().GetFloat("product", 0f);
                int honeyPercentage = Mathf.FloorToInt(current / __instance.m_secPerUnit * 100);
                honeyPercentage = honeyPercentage > 100 ? 100 : honeyPercentage;
                string honey = SetPickableText(honeyPercentage, "", (int) current, (int) __instance.m_secPerUnit);
                string itemName = __instance.m_honeyItem.m_itemData.m_shared.m_name;
                return Localization.instance.Localize(
                    $"{__instance.m_name} {honey}\n{itemName} x {honeyLevel}\n[<color=yellow><b>$KEY_Use</b></color>] $piece_beehive_extract"
                );
            } else return __result;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Fermenter), "GetHoverText")]
        public static string PatchFermenterHoverText(string __result, Fermenter __instance) {
            if (!Main.enableFermenterStatus.Value) return __result;
            if (!PrivateArea.CheckAccess(__instance.transform.position, 0f, false, false))
                return Localization.instance.Localize(__instance.m_name + "\n$piece_noaccess");
            switch (__instance.GetStatus()) {
                case Fermenter.Status.Empty: {
                    if (__instance.m_exposed) {
                        return Localization.instance.Localize(
                            $"{__instance.m_name} ($piece_fermenter_exposed)\n$piece_container_empty"
                        );
                    } else return Localization.instance.Localize(
                        $"{__instance.m_name} ($piece_container_empty)\n[<color=yellow><b>$KEY_Use</b></color>] $piece_fermenter_add"
                    );
                }
                case Fermenter.Status.Fermenting: {
                    string exposedString = __instance.m_exposed ? "\n($piece_fermenter_exposed)" : "";
                    int fermentationPercentage = Mathf.FloorToInt((float) __instance.GetFermentationTime() / __instance.m_fermentationDuration * 100);
                    fermentationPercentage = fermentationPercentage > 100 ? 100 : fermentationPercentage;
                    string fermentation = SetPickableText(fermentationPercentage, "", (int) __instance.GetFermentationTime(), (int) __instance.m_fermentationDuration);
                    return Localization.instance.Localize(
                        $"{__instance.m_name} ($piece_fermenter_fermenting)\n{__instance.GetContentName()} {fermentation}{exposedString}"
                    );
                }
                case Fermenter.Status.Ready: {
                    return Localization.instance.Localize(
                        $"{__instance.m_name} ($piece_fermenter_ready)\n{__instance.GetContentName()}\n[<color=yellow><b>$KEY_Use</b></color>] $piece_fermenter_tap"
                    );
                }
            }
            return __instance.m_name;
        }

        private static void SetHoverText(GameObject go, float current, float total) {
            int percentage = Mathf.RoundToInt(current * 100 / total);
            //Debug.Log($"Health : {current} / {total} ({perc} %)");
            HoverText hoverText = go.GetComponent<HoverText>();
            if (hoverText == null) hoverText = go.AddComponent<HoverText>();
            if (hoverText.m_text.Split('\n').Length == 1) initialText = $"{Hud.instance.m_hoverName.text}\n";
            hoverText.m_text = String.Format(
                initialText + Main.healthStringFormat.Value.Replace("<color>", $"<color={GetColor(percentage)}>"), 
                $"{current:0.#}", 
                total, 
                percentage
            );
            //Chat.instance.SetNpcText(go, Vector3.up, 0, 5.0f, "", $"{current} / {total} ({perc} %)", false);
        }

        private static string SetPickableText(int percentage, string name, int current, int total) {
            string localizedName = Localization.instance.Localize(name);
            string localizedPickUp = Localization.instance.Localize("\n[<color=yellow><b>$KEY_Use</b></color>] $inventory_pickup");
            int remainingTime = current >= total ? 0 : total - current;
            string time = TimeSpan.FromSeconds(remainingTime).ToString(@"hh\:mm\:ss");
            string readyString = Localization.instance.Localize("$piece_fermenter_ready");
            string ready = remainingTime == 0 ? readyString : time;
            return localizedName + " " + String.Format(
                Main.processStringFormat.Value.Replace("<color>", $"<color={GetColor(percentage)}>"),
                percentage,
                ready
            ) + (remainingTime == 0 && name != "" ? localizedPickUp : "");
        }

        private static string GetColor(int percentage) {
            string color = "red";
            if (percentage >= 25 && percentage <= 49) color = "orange";
            if (percentage >= 50 && percentage <= 74) color = "yellow";
            if (percentage >= 75 && percentage <= 100) color = "lime";
            return color;
        }
    }
}