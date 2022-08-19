using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace AlweStats {
    [HarmonyPatch]
    public static class ShipStats {
        private static readonly Dictionary<float, float> shipsMaxSpeed = new() {
            { 300f, 3.1f }, { 500f, 7f }, { 1000f, 9.4f }
        };
        private static Block shipBlock = null;
        private static Ship nearestShip = null;

        public static Block Start() {
            shipBlock = new Block(
                "ShipStats",
                Main.shipStatsColor.Value,
                Main.shipStatsSize.Value,
                Main.shipStatsPosition.Value,
                Main.shipStatsMargin.Value,
                Main.shipStatsAlign.Value
            );
            shipBlock.SetActive(false);
            return shipBlock;
        }

        public static void Update() {
            Player player = Player.m_localPlayer;
            if (player == null) return;
            if (shipBlock != null && nearestShip != null) {
                if (nearestShip.IsPlayerInBoat(Player.m_localPlayer)) {
                    WearNTear wnt = nearestShip.GetComponent<WearNTear>();
                    ZNetView znv = nearestShip.GetComponent<ZNetView>();
                    string windAngle = GetWindAngle(nearestShip.GetWindAngle());
                    float kts = 1.94384f;
                    float windSpeed = EnvMan.instance.GetWindIntensity() * kts * 10f;
                    if (!shipsMaxSpeed.TryGetValue(wnt.m_health, out float maxSpeed)) maxSpeed = wnt.m_health * 7f / 500f;
                    float shipSpeed = Mathf.Abs(nearestShip.GetSpeed() * maxSpeed * kts / 10f);
                    if (wnt != null && znv.IsValid()) {
                        string currentHealth = $"{znv.GetZDO().GetFloat("health", wnt.m_health):0.#}";
                        shipBlock.SetText(string.Format(Main.shipStatsFormat.Value, $"{shipSpeed:0.#} kts", currentHealth, wnt.m_health, $"{windSpeed:0.#} kts", windAngle));
                        shipBlock.SetActive(true);
                    } else shipBlock.SetActive(false);
                } else shipBlock.SetActive(false);
            }
            if (Main.enableShipStatus.Value && nearestShip != null) {
                if (nearestShip.IsPlayerInBoat(Player.m_localPlayer)) MapStats.isOnBoat = true;
                else MapStats.isOnBoat = false;
            }
        }

        private static string GetWindAngle(float angle) {
            int genAngle = (int) Math.Floor(Math.Abs(angle));
            string angleString = "none";
            if (genAngle >= 350 || genAngle <= 10) angleString = $"{genAngle}° N";
            else if (genAngle >= 281 && genAngle <= 349) angleString = $"{genAngle}° NW";
            else if (genAngle >= 261 && genAngle <= 280) angleString = $"{genAngle}° W";
            else if (genAngle >= 191 && genAngle <= 260) angleString = $"{genAngle}° SW";
            else if (genAngle >= 171 && genAngle <= 190) angleString = $"{genAngle}° S";
            else if (genAngle >= 101 && genAngle <= 170) angleString = $"{genAngle}° SE";
            else if (genAngle >= 81 && genAngle <= 100) angleString = $"{genAngle}° E";
            else if (genAngle >= 11 && genAngle <= 80) angleString = $"{genAngle}° NE";
            return angleString;
        }

        private static void Check() {
            Player localPlayer = Player.m_localPlayer;
            if (shipBlock != null && localPlayer != null) {
                try {
                    Ship[] ships = UnityEngine.Object.FindObjectsOfType<Ship>();
                    float pieceDistance = 0f;
                    foreach (Ship s in ships) {
                        float lastDistance = Vector3.Distance(s.transform.position, localPlayer.transform.position);
                        if (nearestShip == null || lastDistance < pieceDistance) nearestShip = s;
                        pieceDistance = lastDistance;
                    }
                } catch (Exception) {}
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Ship), "OnTriggerEnter")]
        static void PatchShipEnter() {
            if (Main.enableShipStats.Value || Main.enableShipStatus.Value) Check();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Ship), "OnDestroyed")]
        static void PatchShipDestroyed() {
            if (Main.enableShipStats.Value && shipBlock != null) shipBlock.SetActive(false);
        }
    }
}
