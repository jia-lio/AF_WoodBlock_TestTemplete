namespace WoodBlock
{
    /// <summary>
    /// 게임 전역 상수 정의
    /// </summary>
    public static class Constants
    {
        // 보드 설정
        public const int BOARD_SIZE = 8;
        public const float CELL_SIZE = 1.0f;
        public const float CELL_SPACING = 0.1f;

        // 블록 설정
        public const int MAX_BLOCK_SLOTS = 3;
        public const int MAX_BLOCK_SIZE = 5;

        // 점수 설정
        public const int SCORE_PER_CELL = 10;
        public const int SCORE_PER_LINE = 100;
        public const float COMBO_MULTIPLIER = 1.5f;

        // 사운드 설정
        public const float DEFAULT_BGM_VOLUME = 0.7f;
        public const float DEFAULT_SFX_VOLUME = 0.8f;

        // 애니메이션 설정
        public const float BLOCK_PLACE_DURATION = 0.2f;
        public const float LINE_CLEAR_DURATION = 0.3f;
        public const float COMBO_DISPLAY_DURATION = 1.0f;

        // 레이어
        public const string LAYER_BOARD = "Board";
        public const string LAYER_BLOCK = "Block";

        // 태그
        public const string TAG_CELL = "Cell";
        public const string TAG_BLOCK = "Block";

        // 저장 키
        public const string SAVE_KEY_BEST_SCORE = "BestScore";
        public const string SAVE_KEY_CURRENT_SCORE = "CurrentScore";
    }
}
