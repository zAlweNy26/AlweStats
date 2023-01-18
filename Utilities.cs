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

        public static T GetCultureInvariant<T>(object num) {
            string str = num.ToString().Replace(",", ".");
            if (typeof(T) == typeof(float)) num = Convert.ToSingle(num, CultureInfo.InvariantCulture);
            else if (typeof(T) == typeof(int)) num = Convert.ToInt32(num, CultureInfo.InvariantCulture);
            return (T) Convert.ChangeType(num, typeof(T));
        }

        public static string VectorToString(Vector2 vector) {
            string x = vector.x.ToString("0.##", CultureInfo.InvariantCulture);
            string y = vector.y.ToString("0.##", CultureInfo.InvariantCulture);
            return $"{x}, {y}";
        }

        public static Vector2 StringToVector(string setting) {
            string[] values = Regex.Replace(setting, @"\s+", "").Split(',');
            if (values.Length < 2) return Vector2.zero;
            return new Vector2(GetCultureInvariant<float>(values[0]), GetCultureInvariant<float>(values[1]));
        }

        public static RectOffset StringToPadding(string setting) {
            string[] values = Regex.Replace(setting, @"\s+", "").Split(',');
            if (values.Length < 4) return new RectOffset(0, 0, 0, 0);
            return new RectOffset(
                GetCultureInvariant<int>(values[0]),
                GetCultureInvariant<int>(values[1]),
                GetCultureInvariant<int>(values[2]),
                GetCultureInvariant<int>(values[3])
            );
        }

        public static Color StringToColor(string setting) {
            string[] values = Regex.Replace(setting, @"\s+", "").Split(',');
            if (values.Length < 4) return Color.gray;
            return new Color(
                Mathf.Clamp01(GetCultureInvariant<float>(values[0]) / 255f),
                Mathf.Clamp01(GetCultureInvariant<float>(values[1]) / 255f),
                Mathf.Clamp01(GetCultureInvariant<float>(values[2]) / 255f),
                Mathf.Clamp01(GetCultureInvariant<float>(values[3]) / 255f)
            );
        }

        public static bool CheckInEnum<T>(T type, string setting) {
            int value = Convert.ToInt32(type);
            int[] values = Array.ConvertAll(Regex.Replace(setting, @"\s+", "").Split(','), s => int.Parse(s, CultureInfo.InvariantCulture));
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

        public static List<WorldInfo> GetWorldInfos() {
            List<WorldInfo> worldsInfos = new();
            if (File.Exists(Main.statsFilePath)) {
                worldsInfos = JsonConvert.DeserializeObject<List<WorldInfo>>(
                    File.ReadAllText(Main.statsFilePath),
                    new JsonSerializerSettings { Error = (se, ev) => { ev.ErrorContext.Handled = true; } }
                );
            }
            return worldsInfos;
        }

        public static void SetWorldInfos(List<WorldInfo> worlds) {
            if (!File.Exists(Main.statsFilePath)) {
                FileStream fs = File.Create(Main.statsFilePath);
                fs.Close();
            }
            string worldsLines = JsonConvert.SerializeObject(
                worlds, 
                Formatting.Indented, 
                new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, NullValueHandling = NullValueHandling.Ignore }
            );
            File.WriteAllText(Main.statsFilePath, worldsLines);
        }

        public static List<WorldInfo> UpdateWorldFile(List<Vector3> pins = null, Dictionary<Minimap.PinData, List<ZDO>> hubs = null) {
            List<WorldInfo> worlds = GetWorldInfos();
            WorldInfo world = worlds.FirstOrDefault(w => w.worldName == ZNet.instance.GetWorldName());
            if (world == null) {
                worlds.Add(new WorldInfo {
                    worldName = ZNet.instance.GetWorldName(),
                    timePlayed = ZNet.instance.GetTimeSeconds(),
                    dayLength = EnvMan.instance.m_dayLengthSec,
                    removedPins = pins == null ? new() : pins,
                    portalsHubs = hubs == null ? new() : hubs
                });
            } else {
                if (pins != null) world.removedPins = pins;
                if (hubs != null) world.portalsHubs = hubs;
            }
            SetWorldInfos(worlds);
            return worlds;
        }

        public static Sprite GetSprite(int nameHash, bool isPiece) {
            if (isPiece) {
                GameObject hammerObj = ObjectDB.instance.GetItemPrefab("Hammer");
                if (!hammerObj) return null;
                ItemDrop hammerDrop = hammerObj.GetComponent<ItemDrop>();
                if (!hammerDrop) return null;
                PieceTable hammerPieceTable = hammerDrop.m_itemData.m_shared.m_buildPieces;
                foreach (GameObject piece in hammerPieceTable.m_pieces) {
                    Piece p = piece.GetComponent<Piece>();
                    if (p.name.GetStableHashCode() == nameHash) return p.m_icon;
                }
            } else {
                ObjectDB.instance.m_itemByHash.TryGetValue(nameHash, out GameObject itemObj);
                if (!itemObj) return null;
                ItemDrop itemDrop = itemObj.GetComponent<ItemDrop>();
                if (!itemDrop) return null;
                return itemDrop.m_itemData.GetIcon();
            }
            return null;
        }
    }

    public enum CustomPinType { Disabled, TrollCave, Crypt, FireHole, Portal, Ship, Cart, MountainCave, RuneStone, InfestedMine }
    public enum EnvType { Disabled, Rock, Tree, Bush, Plant, Beehive, Fermenter, Piece, Fireplace, Container, CookingStation, Smelter }
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
        public Dictionary<Minimap.PinData, List<ZDO>> portalsHubs { get; set; }
        public List<Vector3> removedPins { get; set; }
    }
}
