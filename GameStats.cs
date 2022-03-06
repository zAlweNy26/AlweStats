using System;
using UnityEngine;

namespace AlweStats {
    public static class GameStats {
        private static Block gameBlock = null;

        public static Block Start() {
            gameBlock = new Block(
                "GameStats", 
                Main.gameStatsColor.Value, 
                Main.gameStatsSize.Value, 
                Main.gameStatsPosition.Value,
                Main.gameStatsMargin.Value,
                Main.gameStatsAlign.Value
            );
            return gameBlock;
        }

        public static void Update() {
            if (gameBlock != null) {
                ConnectPanel.instance.UpdateFps();
                float fps = float.Parse(ConnectPanel.instance.m_fps.text);
                if (fps != 9999f) {
                    ZNet.instance.GetNetStats(out var lq, out var rq, out var ping, out var obs, out var ibs);
                    int totalPlayers = ZNet.instance.GetNrOfPlayers();
                    gameBlock.SetText(
                        $"FPS : {fps:0}" +
                        (ping != 0 ? $"\nPing : {ping:0} ms" : "") +
                        (totalPlayers > 1 ? $"\nTotal players : {totalPlayers}" : "")
                    );
                    //Debug.Log($"FPS : {fps} | Ping : {ping:0} ms | Total players : {totalPlayers}");
                }
            }
        }
    }
}
