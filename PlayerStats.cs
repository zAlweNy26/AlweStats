using HarmonyLib;
using UnityEngine;

namespace AlweStats {
    [HarmonyPatch]
    public static class PlayerStats {
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
