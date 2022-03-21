using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

namespace AlweStats {
    public class Utilities {
        public static string GetColorString(float percentage) {
            string color = "";
            if (percentage >= 75f) color = "lime";
            else if (percentage >= 50f) color = "yellow";
            else if (percentage >= 25f) color = "orange";
            else color = "red";
            return color;
        }

        public static string VectorToString(Vector2 v) {
            string x = v.x.ToString("0.##", CultureInfo.InvariantCulture);
            string y = v.y.ToString("0.##", CultureInfo.InvariantCulture);
            return $"{x}, {y}";
        }

        public static Vector2 StringToVector(string s) {
            string[] values = Regex.Replace(s, @"\s+", "").Split(',');
            float x = float.Parse(values[0], CultureInfo.InvariantCulture);
            float y = float.Parse(values[1], CultureInfo.InvariantCulture);
            return new Vector2(x, y);
        }

        public static RectOffset StringToPadding(string s) {
            string[] values = Regex.Replace(s, @"\s+", "").Split(',');
            int left = int.Parse(values[0]);
            int right = int.Parse(values[1]);
            int top = int.Parse(values[2]);
            int bottom = int.Parse(values[3]);
            return new RectOffset(left, right, top, bottom);
        }

        public static Color StringToColor(string s) {
            string[] values = Regex.Replace(s, @"\s+", "").Split(',');
            return new Color(
                Mathf.Clamp01(float.Parse(values[0], CultureInfo.InvariantCulture) / 255f),
                Mathf.Clamp01(float.Parse(values[1], CultureInfo.InvariantCulture) / 255f),
                Mathf.Clamp01(float.Parse(values[2], CultureInfo.InvariantCulture) / 255f),
                Mathf.Clamp01(float.Parse(values[3], CultureInfo.InvariantCulture) / 255f)
            );
        }
    }
}
