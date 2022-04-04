using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Linq;
using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace AlweStats {
    public static class Utilities {

        public static string GetColorString(float percentage) {
            string color = "";
            if (percentage >= 75f) color = "lime";
            else if (percentage >= 50f) color = "yellow";
            else if (percentage >= 25f) color = "orange";
            else color = "red";
            return color;
        }

        public static string VectorToString(Vector2 vector) {
            string x = vector.x.ToString("0.##", CultureInfo.InvariantCulture);
            string y = vector.y.ToString("0.##", CultureInfo.InvariantCulture);
            return $"{x}, {y}";
        }

        public static Vector2 StringToVector(string setting) {
            string[] values = Regex.Replace(setting, @"\s+", "").Split(',');
            if (values.Length < 2) return Vector2.zero;
            float x = float.Parse(values[0], CultureInfo.InvariantCulture);
            float y = float.Parse(values[1], CultureInfo.InvariantCulture);
            return new Vector2(x, y);
        }

        public static RectOffset StringToPadding(string setting) {
            string[] values = Regex.Replace(setting, @"\s+", "").Split(',');
            if (values.Length < 4) return new RectOffset(0, 0, 0, 0);
            int left = int.Parse(values[0]);
            int right = int.Parse(values[1]);
            int top = int.Parse(values[2]);
            int bottom = int.Parse(values[3]);
            return new RectOffset(left, right, top, bottom);
        }

        public static Color StringToColor(string setting) {
            string[] values = Regex.Replace(setting, @"\s+", "").Split(',');
            if (values.Length < 4) return Color.gray;
            return new Color(
                Mathf.Clamp01(float.Parse(values[0], CultureInfo.InvariantCulture) / 255f),
                Mathf.Clamp01(float.Parse(values[1], CultureInfo.InvariantCulture) / 255f),
                Mathf.Clamp01(float.Parse(values[2], CultureInfo.InvariantCulture) / 255f),
                Mathf.Clamp01(float.Parse(values[3], CultureInfo.InvariantCulture) / 255f)
            );
        }

        public static bool CheckInEnum<T>(T type, string setting) {
            int value = Convert.ToInt32(type);
            int[] values = Array.ConvertAll(Regex.Replace(setting, @"\s+", "").Split(','), int.Parse);
            if (value == 0 && values.Contains(0)) return true;
            else if (values.Contains(0)) return false;
            else return values.Contains(value);
        }

        public static Vector3 Round(this Vector3 vector, int decimalPlaces = 1) {
            float multiplier = 1;
            for (int i = 0; i < decimalPlaces; i++) multiplier *= 10f;
            return new Vector3(
                Mathf.Round(vector.x * multiplier) / multiplier,
                Mathf.Round(vector.y * multiplier) / multiplier,
                Mathf.Round(vector.z * multiplier) / multiplier
            );
        }

        public static List<WorldInfo> UpdateWorldFile(List<Vector3> pins = null) {
            if (!File.Exists(Main.statsFilePath)) {
                FileStream fs = File.Create(Main.statsFilePath);
                fs.Close();
            }
            List<WorldInfo> worlds = JsonConvert.DeserializeObject<List<WorldInfo>>(
                File.ReadAllText(Main.statsFilePath),
                new JsonSerializerSettings { Error = (se, ev) => { ev.ErrorContext.Handled = true; } }
            );
            if (pins != null) {
                if (worlds == null) worlds = new();
                WorldInfo world = worlds.FirstOrDefault(w => w.worldName == ZNet.instance.GetWorldName());
                if (world == null) {
                    worlds.Add(new WorldInfo {
                        worldName = ZNet.instance.GetWorldName(),
                        timePlayed = ZNet.instance.GetTimeSeconds(),
                        dayLength = EnvMan.instance.m_dayLengthSec,
                        removedPins = pins
                    });
                } else world.removedPins = pins;
                string worldsLines = JsonConvert.SerializeObject(
                    worlds, 
                    Formatting.Indented, 
                    new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, NullValueHandling = NullValueHandling.Ignore }
                );
                File.WriteAllText(Main.statsFilePath, worldsLines);
            }
            return worlds;
        }
    }

    public enum CustomPinType { Disabled, TrollCave, Crypt, FireHole, Portal, Ship, Cart, MountainCave }
    public enum EnvType { Rock, Tree, Bush, Plant, Beehive, Fermenter, Piece, Fireplace, Container }
    public enum DistanceType { Disabled, Hovering, All }

    public struct CustomPinData {
        public string name;
        public int hash;
        public CustomPinType type;
    }

    public class WorldInfo {
        public string worldName { get; set; }
        public double timePlayed { get; set; }
        public long dayLength { get; set; }
        public List<Vector3> removedPins { get; set; }
    }
}
