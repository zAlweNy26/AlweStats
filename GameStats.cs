using System;
using UnityEngine;

namespace AlweStats {
    public static class GameStats {
        private static Block gameBlock = null;
        private static float frameTimer;
        private static int frameSamples;

        public static Block Start() {
            gameBlock = new Block(
                "GameStats", 
                Main.gameStatsColor.Value, 
                Main.gameStatsSize.Value, 
                Main.gameStatsAlign.Value,
                Main.gameStatsPosition.Value,
                Main.gameStatsMargin.Value
            );
            return gameBlock;
        }

        public static void Update() {
            if (gameBlock != null) {
                string fps = GetFPS();
                ZNet.instance.GetNetStats(out var localQuality, out var remoteQuality, out var ping, out var outByteSec, out var inByteSec);
                if (fps != "0") {
                    //Debug.Log($"FPS : {fps} | Ping : {ping:0} ms");
                    if (ping != 0) gameBlock.SetText($"Ping : {ping:0} ms\nFPS : {fps}");
                    else gameBlock.SetText($"FPS : {fps}");
                }
            }
        }

        private static string GetFPS() {
            string fpsCalculated = "0";
            frameTimer += Time.deltaTime;
            frameSamples++;
            if (frameTimer > 1f) {
                fpsCalculated = Math.Round(1f / (frameTimer / frameSamples)).ToString();
                frameSamples = 0;
                frameTimer = 0f;
            }
            return fpsCalculated;
        }
    }
}
