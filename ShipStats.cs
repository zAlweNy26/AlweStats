using System;
using UnityEngine;

namespace AlweStats {
    public static class ShipStats {
        private static Block shipBlock = null;
        private static bool isOnBoard = false;
        private static Ship nearestShip = null;

        public static Block Start() {
            shipBlock = new Block(
                "ShipStats",
                Main.shipStatsColor.Value,
                Main.shipStatsSize.Value,
                Main.shipStatsAlign.Value,
                Main.shipStatsPosition.Value,
                Main.shipStatsMargin.Value
            );
            return shipBlock;
        }

        public static void Update() {
            if (shipBlock != null && shipBlock.IsActiveAndAlsoParents() && isOnBoard && nearestShip != null) {
                WearNTear wnt = nearestShip.GetComponent<WearNTear>();
                ZNetView znv = nearestShip.GetComponent<ZNetView>();
                //Vector3 windDirection = EnvMan.instance.GetWindDir();
                //Vector3 windForce = EnvMan.instance.GetWindForce();
                string shipHealth = "";
                float windIntensity = EnvMan.instance.GetWindIntensity();
                string windAngle = GetWindAngle(nearestShip.GetWindAngle());
                float windSpeed = windIntensity * 100; // 1 : maximum speed
                float shipSpeed = Math.Max(0, nearestShip.GetSpeed() * 15f); // 3 : maximum speed
                if (wnt && znv?.IsValid() == true) 
                    shipHealth = $"\nShip health : {Mathf.RoundToInt(znv.GetZDO().GetFloat("health", wnt.m_health))} / {Mathf.RoundToInt(wnt.m_health)}";
                //Debug.Log($"Ship speed : {shipSpeed}");
                //Debug.Log($"Ship health : {shipHealth}");
                //Debug.Log($"Wind angle : {windAngle}");
                //Debug.Log($"Wind force : {windForce.x}, {windForce.y}, {windForce.z}");
                //Debug.Log($"Wind intensity : {windIntensity}");
                shipBlock.SetText($"Ship speed : {shipSpeed:0.0} kts{shipHealth}\nWind speed : {windSpeed:0.0} km/h\nWind direction : {windAngle}");
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

        public static void Show() {
            if (shipBlock != null) {
                try {
                    Ship[] ships = UnityEngine.Object.FindObjectsOfType<Ship>();
                    float pieceDistance = 0f;
                    foreach (Ship s in ships) {
                        float lastDistance = Vector3.Distance(s.transform.position, Player.m_localPlayer.transform.position);
                        if (nearestShip == null || lastDistance < pieceDistance) nearestShip = s;
                        pieceDistance = lastDistance;
                    }
                } catch (Exception) {}
                isOnBoard = true;
                shipBlock.SetActive(true);
            }
        }

        public static void Hide() {
            if (shipBlock != null && !Player.m_localPlayer.GetControlledShip()) {
                nearestShip = null;
                isOnBoard = false;
                shipBlock.SetActive(false);
            }
        }
    }
}
