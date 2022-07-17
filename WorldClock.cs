using UnityEngine;
using System;

namespace AlweStats {
    public static class WorldClock {
        private static Block clockBlock = null;

        public static Block Start() {
            clockBlock = new Block(
                "WorldClock",
                Main.worldClockColor.Value,
                Main.worldClockSize.Value,
                Main.worldClockPosition.Value,
                Main.worldClockMargin.Value
            );
            return clockBlock;
        }

        public static void Update() {
            EnvMan envMan = EnvMan.instance;
            if (clockBlock != null && envMan) {
                string format12h = "";
                float minuteFraction = Mathf.Lerp(0f, 24f, envMan.GetDayFraction());
                float floor24h = Mathf.Floor(minuteFraction);
                int hours = Mathf.FloorToInt(floor24h);
                int minutes = Mathf.FloorToInt(Mathf.Lerp(0f, 60f, minuteFraction - floor24h));
                if (Main.worldClockFormat.Value) {
                    format12h = hours < 12 ? "AM" : "PM";
                    if (hours > 12) hours -= 12;
                }
                //Debug.Log($"Clock time : {hours}:{minutes} {format12h}");
                string gameClock = $"{(hours < 10 ? "0" : "")}{hours}:{(minutes < 10 ? "0" : "")}{minutes} {format12h}";
                clockBlock.SetText(gameClock);
            }
        }
    }
}
