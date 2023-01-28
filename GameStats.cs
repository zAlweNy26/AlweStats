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
                    gameBlock.SetText(string.Format(Main.gameStatsFormat.Value, $"{fps:0}"));
                }
            }
        }
    }
}
