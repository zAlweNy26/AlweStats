using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace AlweStats {
    public static class ServerStats {
        private static Block serverBlock = null;

        public static Block Start() {
            serverBlock = new Block(
                "ServerStats", 
                Main.serverStatsColor.Value, 
                Main.serverStatsSize.Value, 
                Main.serverStatsPosition.Value,
                Main.serverStatsMargin.Value,
                Main.serverStatsAlign.Value
            );
            return serverBlock;
        }

        public static void Update() {
            Player localPlayer = Player.m_localPlayer;
            if (serverBlock != null && localPlayer != null) {
                ZNet.instance.GetNetStats(out var lq, out var rq, out var ping, out var obs, out var ibs);
                int totalPlayers = ZNet.instance.GetNrOfPlayers();
                List<Player> playersList = new List<Player>();
                Player.GetPlayersInRange(localPlayer.transform.position, Main.rangeForPlayers.Value, playersList);
                playersList.RemoveAll(player => player.GetPlayerID() == localPlayer.GetPlayerID());
                string[] playersTexts = playersList.Select(p => {
                    string playerName = p.GetPlayerName();
                    int playerHealth = Mathf.CeilToInt(p.GetHealth());
                    int playerMaxHealth = Mathf.CeilToInt(p.GetMaxHealth());
                    float playerHealthPercentage = p.GetHealthPercentage() * 100f;
                    //int playerStamina = Mathf.CeilToInt(p.GetStamina());
                    //int playerMaxStamina = Mathf.CeilToInt(p.GetMaxStamina());
                    //float playerStaminaPercentage = p.GetStaminaPercentage() * 100f;
                    return string.Format(
                        Main.playersInRangeFormat.Value.Replace(
                            "<color>", $"<color={Utilities.GetColorString(playerHealthPercentage)}>"
                        ), playerName, playerHealth, playerMaxHealth, $"{playerHealthPercentage:0.#}"
                    );
                }).ToArray();
                serverBlock.SetText(
                    string.Format(Main.serverStatsFormat.Value, $"{ping:0} ms", totalPlayers) +
                    (playersList.Count > 0 ? $"\n{string.Join("\n", playersTexts)}" : "")
                );
            }
        }
    }
}
