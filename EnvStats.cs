using UnityEngine;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System;

namespace AlweStats {
    [HarmonyPatch]
    public static class EnvStats {
        private static readonly List<string> envObjs = new() { "stub", "bush", "shrub", "oldlog", "beech", "tree", "rock" };
        private static readonly List<string> treeObjs = new() { "stub", "shrub", "oldlog", "beech", "tree" };
        private static string initialText = "";

        private static void SetHoverText(GameObject go, float current, float total) {
            int perc = Mathf.RoundToInt(current * 100 / total);
            //Debug.Log($"Health : {current} / {total} ({perc} %)");
            HoverText hoverText = go.GetComponent<HoverText>();
            if (hoverText == null) hoverText = go.AddComponent<HoverText>();
            if (hoverText.m_text.Split('\n').Length == 1) initialText = $"{Hud.instance.m_hoverName.text}\n";
            hoverText.m_text = String.Format(initialText + Main.envStatsStringFormat.Value, current, total, perc);
            //Chat.instance.SetNpcText(go, Vector3.up, 0, 5.0f, "", $"{current} / {total} ({perc} %)", false);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Destructible), "RPC_Damage")]
        public static void OnDamage(Destructible __instance, HitData hit) {
            if (!Main.enableEnvStats.Value) return;
            List<string> canBeElements = envObjs;
            if (!Main.enableBushStatus.Value) canBeElements.Remove("bush");
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
    }
}