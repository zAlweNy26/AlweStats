using UnityEngine;
using System;

namespace AlweStats {
    public static class SystemClock {
        private static Block clockBlock = null;

        public static Block Start() {
            clockBlock = new Block(
                "SystemClock",
                Main.systemClockColor.Value,
                Main.systemClockSize.Value,
                Main.systemClockPosition.Value,
                Main.systemClockMargin.Value
            );
            return clockBlock;
        }

        public static void Update() {
            if (clockBlock != null) {
                string clockFormat = Main.systemClockFormat.Value ? "hh:mm tt" : "HH:mm";
                string irlClock = DateTime.Now.ToString(clockFormat);
                clockBlock.SetText(irlClock);
            }
        }
    }
}
