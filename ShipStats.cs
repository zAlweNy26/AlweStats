using System;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace AlweStats {
    public static class ShipStats {
        private static GameObject shipObj = null;
        private static bool isOnBoard = false;
        private static Ship nearestShip = null;

        public static void Start() {
            shipObj = new GameObject("ShipStats");
            shipObj.transform.SetParent(Hud.instance.transform.Find("hudroot"));
            shipObj.AddComponent<RectTransform>();
            ContentSizeFitter contentFitter = shipObj.AddComponent<ContentSizeFitter>();
            contentFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            Text shipText = shipObj.AddComponent<Text>();
            string[] colors = Regex.Replace(Main.shipStatsColor.Value, @"\s+", "").Split(',');
            shipText.color = new(
                Mathf.Clamp01(float.Parse(colors[0], CultureInfo.InvariantCulture) / 255f),
                Mathf.Clamp01(float.Parse(colors[1], CultureInfo.InvariantCulture) / 255f),
                Mathf.Clamp01(float.Parse(colors[2], CultureInfo.InvariantCulture) / 255f),
                Mathf.Clamp01(float.Parse(colors[3], CultureInfo.InvariantCulture) / 255f)
            );
            shipText.font = MessageHud.instance.m_messageCenterText.font;
            shipText.fontSize = Main.shipStatsSize.Value;
            shipText.enabled = true;
            Enum.TryParse(Main.shipStatsAlign.Value, out TextAnchor textAlignment);
            shipText.alignment = textAlignment;
            RectTransform statsRect = shipObj.GetComponent<RectTransform>();
            string[] positions = Regex.Replace(Main.shipStatsPosition.Value, @"\s+", "").Split(',');
            statsRect.anchorMax = statsRect.anchorMin = statsRect.pivot = new(
                float.Parse(positions[0], CultureInfo.InvariantCulture),
                float.Parse(positions[1], CultureInfo.InvariantCulture)
            );
            string[] margins = Regex.Replace(Main.shipStatsMargin.Value, @"\s+", "").Split(',');
            statsRect.anchoredPosition = new(
                float.Parse(margins[0], CultureInfo.InvariantCulture),
                float.Parse(margins[1], CultureInfo.InvariantCulture)
            );
            shipObj.SetActive(false);
            isOnBoard = false;
        }

        public static void Update() {
            if (shipObj != null && shipObj.activeInHierarchy && isOnBoard && nearestShip != null) {
                //Vector3 windDirection = EnvMan.instance.GetWindDir();
                //Vector3 windForce = EnvMan.instance.GetWindForce();
                WearNTear wnt = nearestShip.GetComponent<WearNTear>();
                ZNetView znv = nearestShip.GetComponent<ZNetView>();
                float windIntensity = EnvMan.instance.GetWindIntensity();
                string windAngle = GetWindAngle(nearestShip.GetWindAngle());
                float windSpeed = windIntensity * 100; // 1 : maximum speed
                string shipHealth = "";
                float shipSpeed = Math.Max(0, (nearestShip.GetSpeed() * 15f) / 3); // 3 : maximum speed
                if (wnt && znv?.IsValid() == true) shipHealth = $"\nShip health : {Mathf.RoundToInt(znv.GetZDO().GetFloat("health", wnt.m_health))} / {Mathf.RoundToInt(wnt.m_health)}";
                //Debug.Log($"Ship speed : {shipSpeed}");
                //Debug.Log($"Ship health : {shipHealth}");
                //Debug.Log($"Wind angle : {windAngle}");
                //Debug.Log($"Wind force : {windForce.x}, {windForce.y}, {windForce.z}");
                //Debug.Log($"Wind intensity : {windIntensity}");
                shipObj.GetComponent<Text>().text = $"Ship speed : {shipSpeed:0.0} kts{shipHealth}\nWind speed : {windSpeed:0.0} km/h\nWind direction : {windAngle}";
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
            if (shipObj != null) {
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
                shipObj.SetActive(true);
            }
        }

        public static void Hide() {
            if (shipObj != null && !Player.m_localPlayer.GetControlledShip()) {
                nearestShip = null;
                isOnBoard = false;
                shipObj.SetActive(false);
            }
        }
    }
}
