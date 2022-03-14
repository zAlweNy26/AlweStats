using UnityEngine;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.UI;

namespace AlweStats {
    [HarmonyPatch]
    public static class EnvStats {
        private static GameObject pieceObj = null;
        private static readonly List<string> envObjs = new() { "stub", "shrub", "oldlog", "beech", "tree", "rock" };
        private static readonly List<string> treeObjs = new() { "stub", "shrub", "oldlog", "beech", "tree" };
        private static string initialText = "";

        public static void Start() {
            pieceObj = UnityEngine.Object.Instantiate(Hud.instance.m_hoverName.gameObject, Hud.instance.m_hoverName.transform);
            pieceObj.name = "PieceHealthText";
            Hud.instance.m_pieceHealthRoot.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 20f);
            Hud.instance.m_pieceHealthRoot.GetComponent<RectTransform>().rotation = Quaternion.identity;
            pieceObj.transform.SetParent(Hud.instance.m_pieceHealthRoot);
            pieceObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(-50f, 25f);
            pieceObj.GetComponent<RectTransform>().rotation = Quaternion.identity;
            pieceObj.SetActive(false);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Hud), "UpdateCrosshair")]
        public static void PatchHoveringPiece(Hud __instance) {
            if (!Main.enableEnvStats.Value || !Main.enablePieceStatus.Value) return;
            Player localPlayer = Player.m_localPlayer;
            if (localPlayer == null || pieceObj == null) return;
            Piece hoveringPiece = localPlayer.GetHoveringPiece();
            if (hoveringPiece) {
                WearNTear wnt = hoveringPiece.GetComponent<WearNTear>();
                ZNetView znv = hoveringPiece.GetComponent<ZNetView>();
                if (wnt && znv?.IsValid() == true) {
                    float currentHealth = znv.GetZDO().GetFloat("health", wnt.m_health);
                    float currentPercentage = wnt.GetHealthPercentage() * 100f;
                    pieceObj.SetActive(true);
                    __instance.m_pieceHealthBar.SetValue(currentPercentage / 100f);
                    __instance.m_pieceHealthBar.SetColor(Color.Lerp(new Color(1f, 0f, 0f, 1f), new Color(0f, 1f, 0f, 1f), currentPercentage / 100f));
                    pieceObj.GetComponent<Text>().text = String.Format(
                        Main.healthFormat.Value.Replace("<color>", $"<color={GetColor(currentPercentage)}>"), 
                        $"{currentHealth:0.#}", 
                        wnt.m_health, 
                        $"{currentPercentage:0.#}"
                    );
                    //Debug.Log($"{currentHealth:0.#} / {wnt.m_health} ({currentPercentage:0.#} %)");
                } else pieceObj.SetActive(false);
            } else pieceObj.SetActive(false);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Container), "GetHoverText")]
        public static string PatchContainerHoverText(string __result, Container __instance) {
            if (!Main.enableEnvStats.Value || !Main.enableContainerStatus.Value) return __result;
            if (__instance.m_checkGuardStone && !PrivateArea.CheckAccess(__instance.transform.position, 0f, false, false))
                return Localization.instance.Localize(__instance.m_name + "\n$piece_noaccess");
            float perc = __instance.m_inventory.SlotsUsedPercentage();
            int totalSpace = __instance.m_inventory.GetWidth() * __instance.m_inventory.GetHeight();
            string inventoryString = $"{__instance.m_inventory.NrOfItems()} / {totalSpace} (<color={GetColor(perc)}>{perc:0.#} %</color>)";
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
                Hoverable hoverable = __instance.gameObject.GetComponentInParent<Hoverable>();
                string name = "";
                if (hoverable != null) name = hoverable.GetHoverText();
                if (znv.IsValid() && hit.GetTotalDamage() > 0f) {
                    float currentHealth = Mathf.RoundToInt(znv.GetZDO().GetFloat("health", __instance.m_health));
                    SetHoverText(__instance.gameObject, name, currentHealth, __instance.m_health);
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(TreeBase), "RPC_Damage")]
        public static void OnDamage(TreeBase __instance, HitData hit) {
            if (!Main.enableEnvStats.Value || !Main.enableTreeStatus.Value) return;
            //Debug.Log($"TreeBase : {__instance.gameObject.name}");
            ZNetView znv = __instance.m_nview;
            Hoverable hoverable = __instance.gameObject ? __instance.gameObject.GetComponentInParent<Hoverable>() : null;
            if (znv.IsValid() && hit.GetTotalDamage() > 0f && hoverable != null) {
                float currentHealth = Mathf.RoundToInt(znv.GetZDO().GetFloat("health", __instance.m_health));
                SetHoverText(__instance.gameObject, hoverable.GetHoverText(), currentHealth, __instance.m_health);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(TreeLog), "RPC_Damage")]
        public static void OnDamage(TreeLog __instance, HitData hit) {
            if (!Main.enableEnvStats.Value || !Main.enableTreeStatus.Value) return;
            //Debug.Log($"TreeLog : {__instance.gameObject.name}");
            ZNetView znv = __instance.m_nview;
            Hoverable hoverable = __instance.gameObject ? __instance.gameObject.GetComponentInParent<Hoverable>() : null;
            if (znv.IsValid() && hit.GetTotalDamage() > 0f && hoverable != null) {
                float currentHealth = Mathf.RoundToInt(znv.GetZDO().GetFloat("health", __instance.m_health));
                SetHoverText(__instance.gameObject, hoverable.GetHoverText(), currentHealth, __instance.m_health);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MineRock), "RPC_Hit")]
        public static void OnDamage(MineRock __instance, HitData hit, int hitAreaIndex) {
            if (!Main.enableEnvStats.Value || !Main.enableRockStatus.Value) return;
            //Debug.Log($"MineRock : {__instance.gameObject.name}");
            ZNetView znv = __instance.m_nview;
            if (znv.IsValid() && hit.GetTotalDamage() > 0f) {
                float initialTotalHealth = __instance.m_health * __instance.m_hitAreas.Length;
                float currentTotalHealth = __instance.m_hitAreas.Select((a, i) => Math.Max(0, znv.GetZDO().GetFloat($"Health{i}", __instance.m_health))).Sum();
                foreach (Transform t in __instance.transform) SetHoverText(t.gameObject, __instance.m_name, currentTotalHealth, initialTotalHealth);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MineRock5), "RPC_Damage")]
        public static void OnDamage(MineRock5 __instance, HitData hit) {
            if (!Main.enableEnvStats.Value || !Main.enableRockStatus.Value) return;
            //Debug.Log($"MineRock piece : {__instance.gameObject.name}");
            ZNetView znv = __instance.m_nview;
            if (znv.IsValid() && hit.GetTotalDamage() > 0f) {
                float initialTotalHealth = __instance.m_health * __instance.m_hitAreas.Count;
                float currentTotalHealth = __instance.m_hitAreas.Sum(a => Math.Max(0, a.m_health));
                foreach (Transform t in __instance.transform) SetHoverText(t.gameObject, __instance.m_name, currentTotalHealth, initialTotalHealth);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Plant), "GetHoverText")]
        public static string PatchPlantHoverText(string __result, Plant __instance) {
            if (!Main.enableEnvStats.Value || !Main.enablePlantStatus.Value || __instance == null) return __result;
            float growPercentage = (float) __instance.TimeSincePlanted() / __instance.GetGrowTime() * 100f;
            growPercentage = growPercentage > 100f ? 100f : growPercentage;
            return SetPickableText(growPercentage, __instance.GetHoverName(), __instance.GetGrowTime() - __instance.TimeSincePlanted());
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Pickable), "GetHoverText")]
        public static string PatchPickableHoverText(string __result, Pickable __instance) {
            if (!Main.enableEnvStats.Value || !Main.enableBushStatus.Value || !__instance.name.ToLower().Contains("bush")) return __result;
            DateTime startTime = new DateTime(__instance.m_nview.GetZDO().GetLong("picked_time"));
            float currentGrowTime = (float) (ZNet.instance.GetTime() - startTime).TotalSeconds;
            float totalGrowTime = (float) __instance.m_respawnTimeMinutes * 60f;
            float growPercentage = currentGrowTime / totalGrowTime * 100f;
            growPercentage = growPercentage > 100f ? 100f : growPercentage;
            return SetPickableText(growPercentage, __instance.GetHoverName(), totalGrowTime - currentGrowTime);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Beehive), "GetHoverText")]
        public static string PatchBeehiveHoverText(string __result, Beehive __instance) {
            if (!Main.enableEnvStats.Value || !Main.enableBeehiveStatus.Value) return __result;
            if (!PrivateArea.CheckAccess(__instance.transform.position, 0f, false, false))
                return Localization.instance.Localize(__instance.m_name + "\n$piece_noaccess");
            int honeyLevel = __instance.GetHoneyLevel();
            if (honeyLevel > 0) {
                float currentHoneyTime = __instance.GetTimeSinceLastUpdate() + __instance.m_nview.GetZDO().GetFloat("product", 0f);
                float totalHoneyTime = __instance.m_maxHoney * __instance.m_secPerUnit;
                float honeyTimePercentage = (float) honeyLevel / (float) __instance.m_maxHoney * 100f;
                honeyTimePercentage = honeyTimePercentage > 100f ? 100f : honeyTimePercentage;
                string honey = honeyLevel == __instance.m_maxHoney ? 
                    SetPickableText(100f, "", 0f) : SetPickableText(honeyTimePercentage, "", totalHoneyTime - currentHoneyTime);
                string itemName = __instance.m_honeyItem.m_itemData.m_shared.m_name;
                return Localization.instance.Localize(
                    $"{__instance.m_name} {honey}\n{itemName} x {honeyLevel}\n[<color=yellow><b>$KEY_Use</b></color>] $piece_beehive_extract"
                );
            } else return __result;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Fireplace), "GetHoverText")]
        public static string PatchFireplaceHoverText(string __result, Fireplace __instance) {
            if (!Main.enableEnvStats.Value || !Main.enableFireStatus.Value) return __result;
            if (!__instance.m_nview.IsValid()) return "";
            float currentFuel = __instance.m_nview.GetZDO().GetFloat("fuel", 0f);
            if (currentFuel == 0) return __result;
            float currentFuelTime = currentFuel * __instance.m_secPerFuel;
            float totalFuelTime = __instance.m_maxFuel * __instance.m_secPerFuel;
            float fuelTimePercentage = currentFuelTime / totalFuelTime * 100f;
            fuelTimePercentage = fuelTimePercentage > 100f ? 100f : fuelTimePercentage;
            string fuelTime = currentFuel == __instance.m_maxFuel ? 
                SetPickableText(100f, "", 0f) : SetPickableText(fuelTimePercentage, "", currentFuelTime);
            string itemName = __instance.m_fuelItem.m_itemData.m_shared.m_name;
            return Localization.instance.Localize(
                $"{__instance.m_name} ( $piece_fire_fuel {Mathf.Ceil(currentFuel)} / {__instance.m_maxFuel:0} )\n{fuelTime}\n" +
                $"[<color=yellow><b>$KEY_Use</b></color>] $piece_use {itemName}\n[<color=yellow><b>1-8</b></color>] $piece_useitem"
            );
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Fermenter), "GetHoverText")]
        public static string PatchFermenterHoverText(string __result, Fermenter __instance) {
            if (!Main.enableEnvStats.Value || !Main.enableFermenterStatus.Value) return __result;
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
                    float fermentationPercentage = (float) __instance.GetFermentationTime() / __instance.m_fermentationDuration * 100f;
                    fermentationPercentage = fermentationPercentage > 100f ? 100f : fermentationPercentage;
                    string fermentation = SetPickableText(fermentationPercentage, "", __instance.m_fermentationDuration - __instance.GetFermentationTime());
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

        private static void SetHoverText(GameObject go, string name, float current, float total) {
            float percentage = current * 100f / total;
            //Debug.Log($"Health : {current} / {total} ({perc} %)");
            HoverText hoverText = go.GetComponent<HoverText>();
            if (hoverText == null) hoverText = go.AddComponent<HoverText>();
            if (hoverText.m_text.Split('\n').Length == 1) initialText = $"{name}\n";
            hoverText.m_text = String.Format(
                initialText + Main.healthFormat.Value.Replace("<color>", $"<color={GetColor(percentage)}>"), 
                $"{current:0.#}", 
                total, 
                $"{percentage:0.#}"
            );
            //Chat.instance.SetNpcText(go, Vector3.up, 0, 5.0f, "", $"{current} / {total} ({percentage} %)", false);
        }

        private static string SetPickableText(float percentage, string name, double remainingTime) {
            string localizedName = Localization.instance.Localize(name);
            string localizedPickUp = Localization.instance.Localize("\n[<color=yellow><b>$KEY_Use</b></color>] $inventory_pickup");
            string time = TimeSpan.FromSeconds(remainingTime).ToString(@"hh\:mm\:ss");
            string readyString = Localization.instance.Localize("$piece_fermenter_ready");
            string ready = (int) percentage == 100 ? readyString : time;
            return localizedName + " " + String.Format(
                Main.processFormat.Value.Replace("<color>", $"<color={GetColor(percentage)}>"),
                $"{percentage:0.#}",
                ready
            ) + ((int) percentage == 100 && name != "" ? localizedPickUp : "");
        }

        private static string GetColor(float percentage) {
            string color = "red";
            if (percentage >= 25f && percentage <= 49f) color = "orange";
            if (percentage >= 50f && percentage <= 74f) color = "yellow";
            if (percentage >= 75f && percentage <= 100f) color = "lime";
            return color;
        }
    }
}