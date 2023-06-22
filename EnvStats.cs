using UnityEngine;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.UI;
using TMPro;

namespace AlweStats {
    [HarmonyPatch]
    public static class EnvStats {
        private static GameObject pieceObj = null;
        private static readonly List<string> 
            envObjs = new() { "stub", "shrub", "oldlog", "beech", "tree", "rock" },
            treeObjs = new() { "stub", "shrub", "oldlog", "beech", "tree" };

        private static List<string> envObjsNew = new();
        private static string initialText = "";

        public static void Start() {
            envObjsNew = envObjs.Select(i => (string) i.Clone()).ToList();
            if (!Utilities.CheckInEnum(EnvType.Rock, Main.showEnvStatus.Value)) envObjsNew.Remove("rock");
            if (!Utilities.CheckInEnum(EnvType.Tree, Main.showEnvStatus.Value)) envObjsNew = envObjsNew.Except(treeObjs).ToList();
            if (Utilities.CheckInEnum(EnvType.Piece, Main.showEnvStatus.Value)) {
                pieceObj = UnityEngine.Object.Instantiate(Hud.instance.m_hoverName.gameObject, Hud.instance.m_hoverName.transform);
                pieceObj.name = "PieceHealthText";
                Hud.instance.m_pieceHealthRoot.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 20f);
                Hud.instance.m_pieceHealthRoot.GetComponent<RectTransform>().rotation = Quaternion.identity;
                pieceObj.transform.SetParent(Hud.instance.m_pieceHealthRoot);
                pieceObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(-50f, 25f);
                pieceObj.GetComponent<RectTransform>().rotation = Quaternion.identity;
                pieceObj.SetActive(false);
            }
        }
        public static void PatchHoveringPiece() {
            Player localPlayer = Player.m_localPlayer;
            if (localPlayer == null || pieceObj == null) return;
            Piece hoveringPiece = localPlayer.GetHoveringPiece();
            if (hoveringPiece) {
                WearNTear wnt = hoveringPiece.GetComponent<WearNTear>();
                ZNetView znv = hoveringPiece.GetComponent<ZNetView>();
                if (wnt != null && znv.IsValid()) {
                    float currentHealth = znv.GetZDO().GetFloat("health", wnt.m_health);
                    float currentPercentage = wnt.GetHealthPercentage() * 100f;
                    pieceObj.SetActive(true);
                    Hud.instance.m_pieceHealthBar.SetValue(currentPercentage / 100f);
                    Hud.instance.m_pieceHealthBar.SetColor(Color.Lerp(
                        new Color(1f, 0f, 0f, 1f), 
                        new Color(0f, 1f, 0f, 1f), 
                        currentPercentage / 100f
                    ));
                    pieceObj.GetComponent<TextMeshProUGUI>().text = String.Format(
                        Main.healthFormat.Value.Replace("<color>", $"<color={Utilities.GetColorString(currentPercentage)}>"), 
                        $"{currentHealth:0.#}", 
                        wnt.m_health, 
                        $"{currentPercentage:0.#}"
                    );
                } else pieceObj.SetActive(false);
            } else pieceObj.SetActive(false);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Container), nameof(Container.GetHoverText))]
        static void PatchContainerHoverText(ref string __result, Container __instance) {
            if (!Main.enableEnvStats.Value || !Utilities.CheckInEnum(EnvType.Container, Main.showEnvStatus.Value)) return;
            if (__instance.m_checkGuardStone && !PrivateArea.CheckAccess(__instance.transform.position, 0f, false, false)) {
                __result = Localization.instance.Localize(__instance.m_name + "\n$piece_noaccess");
                return;
            }
            float perc = __instance.m_inventory.SlotsUsedPercentage();
            int totalSpace = __instance.m_inventory.GetWidth() * __instance.m_inventory.GetHeight();
            string inventoryString = $"{__instance.m_inventory.NrOfItems()} / {totalSpace} (<color={Utilities.GetColorString(perc)}>{perc:0.#} %</color>)";
            string notEmpty = __instance.m_inventory.NrOfItems() > 0 ? $"\n{inventoryString}" : " ( $piece_container_empty )";
            __result = Localization.instance.Localize($"{__instance.m_name}{notEmpty}\n[<color=yellow><b>$KEY_Use</b></color>] $piece_container_open");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Destructible), nameof(Destructible.RPC_Damage))]
        static void OnDamage(Destructible __instance, HitData hit) {
            if (!Main.enableEnvStats.Value || __instance == null) return;
            if (envObjsNew.Any(e => __instance.gameObject.name.ToLower().Contains(e))) {
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
        [HarmonyPatch(typeof(TreeBase), methodName: nameof(TreeBase.RPC_Damage))]
        static void OnDamage(TreeBase __instance, HitData hit) {
            if (!Main.enableEnvStats.Value || !Utilities.CheckInEnum(EnvType.Tree, Main.showEnvStatus.Value)) return;
            ZNetView znv = __instance.m_nview;
            Hoverable hoverable = __instance.gameObject ? __instance.gameObject.GetComponentInParent<Hoverable>() : null;
            if (znv.IsValid() && hit.GetTotalDamage() > 0f && hoverable != null) {
                float currentHealth = Mathf.RoundToInt(znv.GetZDO().GetFloat("health", __instance.m_health));
                SetHoverText(__instance.gameObject, hoverable.GetHoverText(), currentHealth, __instance.m_health);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(TreeLog), methodName: nameof(TreeLog.RPC_Damage))]
        static void OnDamage(TreeLog __instance, HitData hit) {
            if (!Main.enableEnvStats.Value || !Utilities.CheckInEnum(EnvType.Tree, Main.showEnvStatus.Value)) return;
            ZNetView znv = __instance.m_nview;
            Hoverable hoverable = __instance.gameObject ? __instance.gameObject.GetComponentInParent<Hoverable>() : null;
            if (znv.IsValid() && hit.GetTotalDamage() > 0f && hoverable != null) {
                float currentHealth = Mathf.RoundToInt(znv.GetZDO().GetFloat("health", __instance.m_health));
                SetHoverText(__instance.gameObject, hoverable.GetHoverText(), currentHealth, __instance.m_health);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MineRock), nameof(MineRock.RPC_Hit))]
        static void OnDamage(MineRock __instance, HitData hit, int hitAreaIndex) {
            if (!Main.enableEnvStats.Value || !Utilities.CheckInEnum(EnvType.Rock, Main.showEnvStatus.Value)) return;
            ZNetView znv = __instance.m_nview;
            if (znv.IsValid() && hit.GetTotalDamage() > 0f) {
                float initialTotalHealth = __instance.m_health * __instance.m_hitAreas.Length;
                float currentTotalHealth = __instance.m_hitAreas.Select((a, i) => Math.Max(0, znv.GetZDO().GetFloat($"Health{i}", __instance.m_health))).Sum();
                foreach (Transform t in __instance.transform) SetHoverText(t.gameObject, __instance.m_name, currentTotalHealth, initialTotalHealth);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MineRock5), nameof(MineRock5.RPC_Damage))]
        static void OnDamage(MineRock5 __instance, HitData hit) {
            if (!Main.enableEnvStats.Value || !Utilities.CheckInEnum(EnvType.Rock, Main.showEnvStatus.Value)) return;
            ZNetView znv = __instance.m_nview;
            if (znv.IsValid() && hit.GetTotalDamage() > 0f) {
                float initialTotalHealth = __instance.m_health * __instance.m_hitAreas.Count;
                float currentTotalHealth = __instance.m_hitAreas.Sum(a => Math.Max(0, a.m_health));
                foreach (Transform t in __instance.transform) SetHoverText(t.gameObject, __instance.m_name, currentTotalHealth, initialTotalHealth);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Plant), nameof(Plant.GetHoverText))]
        static void PatchPlantHoverText(ref string __result, Plant __instance) {
            if (!Main.enableEnvStats.Value || !Utilities.CheckInEnum(EnvType.Plant, Main.showEnvStatus.Value) || __instance == null) return;
            if (!__instance.m_nview.IsValid()) return;
            float growPercentage = (float) __instance.TimeSincePlanted() / __instance.GetGrowTime() * 100f;
            growPercentage = growPercentage > 100f ? 100f : growPercentage;
            __result = SetPickableText(growPercentage, __instance.GetHoverName(), __instance.GetGrowTime() - __instance.TimeSincePlanted());
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Pickable), nameof(Pickable.GetHoverText))]
        static void PatchPickableHoverText(ref string __result, Pickable __instance) {
            if (!Main.enableEnvStats.Value || !Utilities.CheckInEnum(EnvType.Bush, Main.showEnvStatus.Value) 
                || !__instance.name.ToLower().Contains("bush")) return;
            DateTime startTime = new DateTime(__instance.m_nview.GetZDO().GetLong("picked_time"));
            float currentGrowTime = (float) (ZNet.instance.GetTime() - startTime).TotalSeconds;
            float totalGrowTime = (float) __instance.m_respawnTimeMinutes * 60f;
            float growPercentage = currentGrowTime / totalGrowTime * 100f;
            growPercentage = growPercentage > 100f ? 100f : growPercentage;
            __result = SetPickableText(growPercentage, __instance.GetHoverName(), totalGrowTime - currentGrowTime);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Beehive), nameof(Beehive.GetHoverText))]
        static void PatchBeehiveHoverText(ref string __result, Beehive __instance) {
            if (!Main.enableEnvStats.Value || !Utilities.CheckInEnum(EnvType.Beehive, Main.showEnvStatus.Value)) return;
            if (!PrivateArea.CheckAccess(__instance.transform.position, 0f, false, false)) {
                __result = Localization.instance.Localize(__instance.m_name + "\n$piece_noaccess");
                return;
            }
            int honeyLevel = __instance.GetHoneyLevel();
            if (honeyLevel > 0) {
                float currentHoneyTime = __instance.GetTimeSinceLastUpdate() + __instance.m_nview.GetZDO().GetFloat("product", 0f);
                float totalHoneyTime = __instance.m_maxHoney * __instance.m_secPerUnit;
                float honeyTimePercentage = (float) honeyLevel / (float) __instance.m_maxHoney * 100f;
                honeyTimePercentage = honeyTimePercentage > 100f ? 100f : honeyTimePercentage;
                string honey = honeyLevel == __instance.m_maxHoney ? 
                    SetPickableText(100f, "", 0f) : SetPickableText(honeyTimePercentage, "", totalHoneyTime - currentHoneyTime);
                string itemName = __instance.m_honeyItem.m_itemData.m_shared.m_name;
                __result = Localization.instance.Localize(
                    $"{__instance.m_name} {honey}\n{itemName} x {honeyLevel}\n[<color=yellow><b>$KEY_Use</b></color>] $piece_beehive_extract"
                );
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Fireplace), nameof(Fireplace.GetHoverText))]
        static void PatchFireplaceHoverText(ref string __result, Fireplace __instance) {
            if (!Main.enableEnvStats.Value || !Utilities.CheckInEnum(EnvType.Fireplace, Main.showEnvStatus.Value)) return;
            if (!__instance.m_nview.IsValid()) {
                __result = "";
                return;
            }
            float currentFuel = __instance.m_nview.GetZDO().GetFloat("fuel", 0f);
            if (currentFuel == 0) return;
            float currentFuelTime = currentFuel * __instance.m_secPerFuel;
            float totalFuelTime = __instance.m_maxFuel * __instance.m_secPerFuel;
            float fuelTimePercentage = currentFuelTime / totalFuelTime * 100f;
            fuelTimePercentage = fuelTimePercentage > 100f ? 100f : fuelTimePercentage;
            string fuelTime = currentFuel == __instance.m_maxFuel ? 
                SetPickableText(100f, "", 0f) : SetPickableText(fuelTimePercentage, "", currentFuelTime);
            string itemName = __instance.m_fuelItem.m_itemData.m_shared.m_name;
            __result = Localization.instance.Localize(
                $"{__instance.m_name} ( $piece_fire_fuel {Mathf.Ceil(currentFuel)} / {__instance.m_maxFuel:0} )\n{fuelTime}\n" +
                $"[<color=yellow><b>$KEY_Use</b></color>] $piece_use {itemName}\n[<color=yellow><b>1-8</b></color>] $piece_useitem"
            );
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CookingStation), nameof(CookingStation.GetHoverText))]
        static void PatchCookingStationHoverText(ref string __result, CookingStation __instance) {
            if (!Main.enableEnvStats.Value || !Utilities.CheckInEnum(EnvType.CookingStation, Main.showEnvStatus.Value)) return;
            if (!__instance.m_nview.IsValid()) {
                __result = "";
                return;
            }
            string cookingItems = "";
            int items = 0;
            for (int i = 0; i < __instance.m_slots.Length; i++) {
                __instance.GetSlot(i, out string text, out float num, out CookingStation.Status status);
                if (text != "" && text != __instance.m_overCookedItem.name) {
                    CookingStation.ItemConversion itemConversion = __instance.GetItemConversion(text);
                    if (text != null) {
                        items++;
                        if (num > itemConversion.m_cookTime) {
                            string time = $"{((num - itemConversion.m_cookTime) * 100f / (itemConversion.m_cookTime)):0.#} %";
                            cookingItems += $"\n{itemConversion.m_to.GetHoverName()}: <color=red>{time}</color>";
                        } else {
                            string time = $"{(num * 100f / (itemConversion.m_cookTime)):0.#} %";
                            cookingItems += $"\n{itemConversion.m_from.GetHoverName()}: <color=green>{time}</color>";
                        }
                    }
                }
            }
            if (items > 0) {
                __result = items >= __instance.m_slots.Length ?
                    Localization.instance.Localize($"{__instance.m_name}{cookingItems}") :
                    Localization.instance.Localize($"{__instance.m_name}" + 
                        $"\n[<color=yellow><b>$KEY_Use</b></color>] {__instance.m_addItemTooltip}" + 
                        $"\n[<color=yellow><b>1-8</b></color>] {__instance.m_addItemTooltip}{cookingItems}");
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Smelter), nameof(Smelter.OnHoverAddFuel))]
        static void PatchSmelterHoverAddFuel(ref string __result, Smelter __instance) {
            if (!Main.enableEnvStats.Value || !Utilities.CheckInEnum(EnvType.Smelter, Main.showEnvStatus.Value)) return;
            if (!__instance.m_nview.IsValid()) {
                __result = "";
                return;
            }
            float fuel = __instance.GetFuel();
            string itemName = __instance.m_fuelItem.m_itemData.m_shared.m_name;
            __result = string.Format("{0} ({1} {2}/{3})\n", __instance.m_name, itemName, Mathf.Ceil(fuel), __instance.m_maxFuel);
            if (fuel > 0f) {
                float secPerProduct = __instance.m_secPerProduct * __instance.m_fuelPerProduct * (Main.showTotalOfQueue.Value ? fuel : 1f);
                float percentage = (Main.showTotalOfQueue.Value ? fuel / __instance.m_maxFuel : (float)(fuel - Math.Truncate(fuel))) * 100f;
                __result += String.Format(
                    Main.processFormat.Value.Replace("<color>", $"<color={Utilities.GetColorString(percentage)}>"),
                    $"{percentage:0.#}",
                    TimeSpan.FromSeconds(secPerProduct).ToString(@"hh\:mm\:ss")
                );
                __result += "\n";
            }
            __result += $"[<color=yellow><b>$KEY_Use</b></color>] $piece_smelter_add {itemName}";
            __result = Localization.instance.Localize(__result);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Smelter), nameof(Smelter.OnHoverAddOre))]
        static void PatchSmelterHoverAddOre(ref string __result, Smelter __instance) {
            if (!Main.enableEnvStats.Value || !Utilities.CheckInEnum(EnvType.Smelter, Main.showEnvStatus.Value)) return;
            if (!__instance.m_nview.IsValid()) {
                __result = "";
                return;
            }
            __instance.m_sb.Clear();
            int queueSize = __instance.GetQueueSize();
            __instance.m_sb.Append($"{__instance.m_name} ({queueSize}/{__instance.m_maxOre})\n");
            if (queueSize > 0) {
                float percentage = __instance.GetBakeTimer() * 100f / (__instance.m_secPerProduct * (Main.showTotalOfQueue.Value ? queueSize : 1f));
                double remainingTime = (__instance.m_secPerProduct * (Main.showTotalOfQueue.Value ? queueSize : 1f)) - __instance.GetBakeTimer();
                __instance.m_sb.Append(String.Format(
                    Main.processFormat.Value.Replace("<color>", $"<color={Utilities.GetColorString(percentage)}>"),
                    $"{percentage:0.#}",
                    TimeSpan.FromSeconds(remainingTime).ToString(@"hh\:mm\:ss")
                ));
                __instance.m_sb.Append("\n");
            }
            if (__instance.m_requiresRoof && !__instance.m_haveRoof && Mathf.Sin(Time.time * 10f) > 0f)
                __instance.m_sb.Append("<color=yellow>$piece_smelter_reqroof</color>\n");
            __instance.m_sb.Append("[<color=yellow><b>$KEY_Use</b></color>] " + __instance.m_addOreTooltip);
            __result = Localization.instance.Localize(__instance.m_sb.ToString());
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Fermenter), nameof(Fermenter.GetHoverText))]
        static void PatchFermenterHoverText(ref string __result, Fermenter __instance) {
            if (!Main.enableEnvStats.Value || !Utilities.CheckInEnum(EnvType.Fermenter, Main.showEnvStatus.Value)) return;
            if (!PrivateArea.CheckAccess(__instance.transform.position, 0f, false, false))
                __result = Localization.instance.Localize(__instance.m_name + "\n$piece_noaccess");
            switch (__instance.GetStatus()) {
                case Fermenter.Status.Empty: {
                    if (__instance.m_exposed) __result = Localization.instance.Localize(
                            $"{__instance.m_name} ($piece_fermenter_exposed)\n$piece_container_empty"
                        );
                    else __result = Localization.instance.Localize(
                        $"{__instance.m_name} ($piece_container_empty)\n[<color=yellow><b>$KEY_Use</b></color>] $piece_fermenter_add"
                    );
                    return;
                }
                case Fermenter.Status.Fermenting: {
                    string exposedString = __instance.m_exposed ? "\n($piece_fermenter_exposed)" : "";
                    float fermentationPercentage = (float) __instance.GetFermentationTime() / __instance.m_fermentationDuration * 100f;
                    fermentationPercentage = fermentationPercentage > 100f ? 100f : fermentationPercentage;
                    string fermentation = SetPickableText(fermentationPercentage, "", __instance.m_fermentationDuration - __instance.GetFermentationTime());
                    __result = Localization.instance.Localize(
                        $"{__instance.m_name} ($piece_fermenter_fermenting)\n{__instance.GetContentName()} {fermentation}{exposedString}"
                    );
                    return;
                }
                case Fermenter.Status.Ready: {
                    __result = Localization.instance.Localize(
                        $"{__instance.m_name} ($piece_fermenter_ready)\n{__instance.GetContentName()}\n[<color=yellow><b>$KEY_Use</b></color>] $piece_fermenter_tap"
                    );
                    return;
                }
            }
            __result = __instance.m_name;
        }

        private static void SetHoverText(GameObject go, string name, float current, float total) {
            float percentage = current * 100f / total;
            HoverText hoverText = go.GetComponent<HoverText>();
            if (hoverText == null) hoverText = go.AddComponent<HoverText>();
            if (hoverText.m_text.Split('\n').Length == 1) initialText = $"{name}\n";
            hoverText.m_text = String.Format(
                initialText + Main.healthFormat.Value.Replace("<color>", $"<color={Utilities.GetColorString(percentage)}>"), 
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
                Main.processFormat.Value.Replace("<color>", $"<color={Utilities.GetColorString(percentage)}>"),
                $"{percentage:0.#}",
                ready
            ) + ((int) percentage == 100 && name != "" ? localizedPickUp : "");
        }
    }
}