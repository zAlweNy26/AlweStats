using System;
using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;
using UnityEngine.UI;

namespace AlweStats {
    [HarmonyPatch]
    public static class EntityStats {
        public static void Start() {
            Transform originalHealthText = EnemyHud.instance.m_baseHudMount.transform.Find("Health/HealthText");
            Transform originalName = EnemyHud.instance.m_baseHudMount.transform.Find("Name");
            foreach (Transform t in EnemyHud.instance.m_hudRoot.transform) {
                Transform healthTransform = t.Find("Health");
                Transform nameTransform = t.Find("Name");   
                if (Main.enableEntityStats.Value && healthTransform) {
                    if (healthTransform.Find("HealthText")) return;
                    GameObject healthObj = null;
                    if (Main.HasAuga) {
                        healthObj = UnityEngine.Object.Instantiate(nameTransform.gameObject, healthTransform);
                    } else {
                        healthObj = UnityEngine.Object.Instantiate(originalHealthText.gameObject, originalHealthText);
                    }
                    
                    if (healthObj) {
                        healthObj.name = "HealthText";
                        healthObj.transform.SetParent(healthTransform);
                        healthObj.GetComponent<RectTransform>().anchoredPosition = Vector2.up;
                        healthObj.SetActive(true);
                        healthObj.GetComponent<Text>().fontSize = Main.healthBarHeight.Value;
                    }

                    Vector2 darkenRect = healthTransform.Find("darken").GetComponent<RectTransform>().sizeDelta;
                    healthTransform.Find("darken").GetComponent<RectTransform>().sizeDelta = 
                        new(darkenRect.x, Main.healthBarHeight.Value + 3);

                    Vector2 bgRect = healthTransform.Find("bkg").GetComponent<RectTransform>().sizeDelta;
                    healthTransform.Find("bkg").GetComponent<RectTransform>().sizeDelta = 
                        new(bgRect.x, Math.Max(Main.healthBarHeight.Value - 4, 0));

                    Vector2 slowRect = healthTransform.Find("health_slow/bar").GetComponent<RectTransform>().sizeDelta;
                    healthTransform.Find("health_slow/bar").GetComponent<RectTransform>().sizeDelta = 
                        new(slowRect.x, Main.healthBarHeight.Value);

                    Vector2 fastRect = healthTransform.Find("health_fast/bar").GetComponent<RectTransform>().sizeDelta;
                    healthTransform.Find("health_fast/bar").GetComponent<RectTransform>().sizeDelta = 
                        new(fastRect.x, Main.healthBarHeight.Value);
                }
                if (!Utilities.CheckInEnum(DistanceType.Disabled, Main.showEntityDistance.Value) && nameTransform) {
                    GameObject distanceObj = UnityEngine.Object.Instantiate(originalName.gameObject, originalName);
                    distanceObj.name = "Distance";
                    distanceObj.transform.SetParent(nameTransform.parent);
                    Vector2 originalNamePos = nameTransform.GetComponent<RectTransform>().anchoredPosition;
                    distanceObj.GetComponent<Text>().fontSize = 14;
                    distanceObj.GetComponent<Text>().text = "0 m";
                    distanceObj.GetComponent<RectTransform>().anchoredPosition = new(originalNamePos.x, -(originalNamePos.y / 2));
                    distanceObj.SetActive(false);
                }
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(EnemyHud), nameof(EnemyHud.ShowHud))]
        static bool ShowHud(ref EnemyHud __instance, Character c, bool isMount) {
            if (!Main.enableEntityStats.Value) return true;
            EnemyHud.HudData hudData;
            if (__instance.m_huds.TryGetValue(c, out hudData)) return false;
            GameObject original;
            if (isMount) original = __instance.m_baseHudMount;
            else if (c.IsPlayer()) original = __instance.m_baseHudPlayer;
            else if (c.IsBoss()) original = __instance.m_baseHudBoss;
            else original = __instance.m_baseHud;
            hudData = new EnemyHud.HudData();
            hudData.m_character = c;
            hudData.m_ai = c.GetComponent<BaseAI>();
            hudData.m_gui = UnityEngine.Object.Instantiate<GameObject>(original, __instance.m_hudRoot.transform);
            hudData.m_gui.SetActive(true);
            hudData.m_healthFast = hudData.m_gui.transform.Find("Health/health_fast").GetComponent<GuiBar>();
            hudData.m_healthSlow = hudData.m_gui.transform.Find("Health/health_slow").GetComponent<GuiBar>();
            Transform healthFastFriendly = hudData.m_gui.transform.Find("Health/health_fast_friendly");
            if (healthFastFriendly) {
                hudData.m_healthFastFriendly = healthFastFriendly.GetComponent<GuiBar>();
            }
            if (isMount) {
                hudData.m_stamina = hudData.m_gui.transform.Find("Stamina/stamina_fast").GetComponent<GuiBar>();
                hudData.m_staminaText = hudData.m_gui.transform.Find("Stamina/StaminaText").GetComponent<Text>();
            }
            hudData.m_healthText = hudData.m_gui.transform.Find("Health/HealthText").GetComponent<Text>();
            hudData.m_level2 = hudData.m_gui.transform.Find("level_2") as RectTransform;
            hudData.m_level3 = hudData.m_gui.transform.Find("level_3") as RectTransform;
            hudData.m_alerted = hudData.m_gui.transform.Find("Alerted") as RectTransform;
            hudData.m_aware = hudData.m_gui.transform.Find("Aware") as RectTransform;
            hudData.m_name = hudData.m_gui.transform.Find("Name").GetComponent<Text>();
            hudData.m_name.text = Localization.instance.Localize(c.GetHoverName());
            hudData.m_isMount = isMount;
            __instance.m_huds.Add(c, hudData);
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Character), nameof(Character.GetHoverText))]
        static void PatchEntityHoverText(ref string __result, Character __instance) {
            if (!Main.enableEntityStats.Value) return;
            if (!__instance.m_nview.IsValid()) {
                __result = "";
                return;
            }
            Procreation procreation = __instance.m_baseAI.GetComponent<Procreation>();
            Tameable tameable = __instance.GetComponent<Tameable>();
            if (procreation && procreation.IsPregnant() && tameable) {
                float pregnancyDur = procreation.m_pregnancyDuration;
                long pregnancyProgress = __instance.m_nview.GetZDO().GetLong("pregnant", 0L);
                double pregnancyPerc = Math.Round((ZNet.instance.GetTime() - new DateTime(pregnancyProgress)).TotalSeconds / pregnancyDur * 100.0, 1);
                __result += Localization.instance.Localize($"\n$alwe_pregnant : {pregnancyPerc:0.#} %");
                /*__result = Localization.instance.Localize(__instance.m_name);
                if (__instance.IsTamed()) {
                    __result += Localization.instance.Localize(" ( $hud_tame, " + tameable.GetStatusString() + " )");
                    __result += $"\nPregnant : {pregnancyPerc:0.#} %";
                    __result += Localization.instance.Localize("\n[<color=yellow><b>$KEY_Use</b></color>] $hud_pet");
                    __result += Localization.instance.Localize("\n[<color=yellow><b>$KEY_AltPlace + $KEY_Use</b></color>] $hud_rename");
                    return;
                }
                int tameness = tameable.GetTameness();
                if (tameness <= 0) {
                    __result += Localization.instance.Localize(" ( $hud_wild, " + tameable.GetStatusString() + " )");
                } else {
                    __result += Localization.instance.Localize(string.Concat(new string[] {
                        " ( $hud_tameness  ", tameness.ToString(), "%, ", tameable.GetStatusString(), " )"
                    }));
                }*/
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(EnemyHud), nameof(EnemyHud.UpdateHuds))]
        static void UpdateHuds(ref EnemyHud __instance, Player player, Sadle sadle, float dt) {
            if (!Main.enableEntityStats.Value) return;
            Character character = null;
            Character hoverCreature = player ? player.GetHoverCreature() : null;
            foreach (KeyValuePair<Character, EnemyHud.HudData> keyValuePair in __instance.m_huds) {
                EnemyHud.HudData value = keyValuePair.Value;
                if (!value.m_character || !__instance.TestShow(value.m_character, true)) {
                    if (character == null) {
                        character = value.m_character;
                        UnityEngine.Object.Destroy(value.m_gui);
                    }
                } else {
                    if (value.m_isMount && sadle.GetCharacter()) {
                        float staminaPercentage = sadle.GetCharacter().GetStaminaPercentage() * 100f;
                        int currentStamina = Mathf.CeilToInt(sadle.GetStamina());
                        int totalStamina = Mathf.CeilToInt(sadle.GetMaxStamina());
                        value.m_staminaText.text = String.Format(
                            Main.healthFormat.Value.Replace("<color>", $"<color={Utilities.GetColorString(staminaPercentage)}>"), 
                            currentStamina, 
                            totalStamina, 
                            $"{staminaPercentage:0.#}"
                        );
                    }
                    float healthPercentage = value.m_character.GetHealthPercentage() * 100f;
                    int currentHealth = Mathf.CeilToInt(value.m_character.GetHealth());
                    int totalHealth = Mathf.CeilToInt(value.m_character.GetMaxHealth());
                    value.m_healthText.text = String.Format(
                        Main.healthFormat.Value.Replace("<color>", $"<color={Utilities.GetColorString(healthPercentage)}>"), 
                        currentHealth, 
                        totalHealth, 
                        $"{healthPercentage:0.#}"
                    );
                    if (Utilities.CheckInEnum(DistanceType.Hovering, Main.showEntityDistance.Value) && player) {
                        if (hoverCreature == value.m_character) {
                            float distance = Vector3.Distance(player.transform.position, hoverCreature.transform.position);
                            value.m_gui.transform.Find("Distance").GetComponent<Text>().text = $"{distance:0.#} m";
                            value.m_gui.transform.Find("Distance").gameObject.SetActive(true);
                        } else value.m_gui.transform.Find("Distance").gameObject.SetActive(false);
                    }
                    if (Utilities.CheckInEnum(DistanceType.All, Main.showEntityDistance.Value) && player) {
                        float distance = Vector3.Distance(player.transform.position, value.m_character.transform.position);
                        value.m_gui.transform.Find("Distance").GetComponent<Text>().text = $"{distance:0.#} m";
                        value.m_gui.transform.Find("Distance").gameObject.SetActive(true);
                    }
                    value.m_gui.transform.Find("Health/health_fast").GetComponent<GuiBar>().SetColor(Utilities.StringToColor(Main.healthBarColor.Value));
                    value.m_gui.transform.Find("Health/health_slow").GetComponent<GuiBar>().SetColor(Utilities.StringToColor(Main.healthBarColor.Value));
                    if (value.m_character.IsTamed()) {
                        value.m_gui.transform.Find("Health/health_fast").GetComponent<GuiBar>().SetColor(Utilities.StringToColor(Main.tamedBarColor.Value));
                        value.m_gui.transform.Find("Health/health_slow").GetComponent<GuiBar>().SetColor(Utilities.StringToColor(Main.tamedBarColor.Value));
                    }
                    if (value.m_healthFastFriendly) {
                        value.m_healthFastFriendly.gameObject.SetActive(value: false);
                    }
                }
            }
            if (character) __instance.m_huds.Remove(character);
        }
    }
}
