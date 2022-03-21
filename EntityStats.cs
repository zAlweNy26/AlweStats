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
            foreach (Transform t in EnemyHud.instance.m_hudRoot.transform) {
                Transform child = t.Find("Health");
                if (child != null) {
                    if (child.Find("HealthText")) return;
                    GameObject healthObj = UnityEngine.Object.Instantiate(originalHealthText.gameObject, originalHealthText);
                    healthObj.name = "HealthText";
                    healthObj.transform.SetParent(child);
                    healthObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 1f);
                    healthObj.SetActive(true);
                    Transform darken = child.Find("darken");
                    Transform background = child.Find("bkg");
                    Transform slow = child.Find("health_slow/bar");
                    Transform fast = child.Find("health_fast/bar");
                    if (slow != null) {
                        if (slow.GetComponent<RectTransform>().sizeDelta.y < 12) {
                            slow.GetComponent<RectTransform>().sizeDelta = new Vector2(100f, 12f);
                            if (fast != null) fast.GetComponent<RectTransform>().sizeDelta = new Vector2(100f, 12f);
                            if (darken != null) darken.GetComponent<RectTransform>().sizeDelta = new Vector2(15f, 15f);
                            if (background != null) background.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 8f);
                        }
                        healthObj.GetComponent<Text>().fontSize = (int) slow.GetComponent<RectTransform>().sizeDelta.y;
                    }
                }
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(EnemyHud), "ShowHud")]
        public static bool ShowHud(ref EnemyHud __instance, Character c, bool isMount) {
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
        [HarmonyPatch(typeof(EnemyHud), "UpdateHuds")]
        private static void UpdateHuds(ref EnemyHud __instance, Player player, Sadle sadle, float dt) {
            if (!Main.enableEntityStats.Value) return;
            Character character = null;
            foreach (KeyValuePair<Character, EnemyHud.HudData> keyValuePair in __instance.m_huds) {
                EnemyHud.HudData value = keyValuePair.Value;
                if (!value.m_character || !__instance.TestShow(value.m_character)) {
                    if (character == null) {
                        character = value.m_character;
                        UnityEngine.Object.Destroy(value.m_gui);
                    }
                } else {
                    if (value.m_isMount) {
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
                    /*
                    Player player = (Player) c;
                    float carryWeightPercentage = player.GetInventory().GetTotalWeight() / player.GetMaxCarryWeight() * 100f;
                    Debug.Log($"Carry weight : {player.GetInventory().GetTotalWeight()} / {player.GetMaxCarryWeight()} ({carryWeightPercentage:0.#} %)");
                    Debug.Log($"Stamina : {player.GetStamina()} / {player.GetMaxStamina()} ({player.GetStaminaPercentage()} %)");
                    Debug.Log($"Level : {player.GetLevel()}");
                    Debug.Log($"Armor : {player.GetBodyArmor()}");
                    Debug.Log($"Owner : {player.GetOwner()}");
                    Debug.Log($"God mode : {player.InGodMode()}");
                    */
                    //Debug.Log($"Health : {currentHealth:0.#} / {totalHealth} ({healthPercentage:0.#} %)");
                }
            }
            if (character != null) __instance.m_huds.Remove(character);
        }
    }
}
