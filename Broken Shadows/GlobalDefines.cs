namespace Broken_Shadows
{
    public class GlobalDefines
    {
        public static int WINDOW_WIDTH = 896;
        public static int WINDOW_HEIGHT = 832;
        public static int MOUSE_CURSOR_SIZE = 32;

        public static int MOVEMENT_FRAMES = 8;
        public static float TILE_SIZE = 64;
        public static float PLAYER_SIZE = 52;
        public static float PLAYER_OFFSET = (TILE_SIZE - PLAYER_SIZE) / 2;
        public static int GRID_SIZE = 11;
        public static float TILE_STEP_SIZE = TILE_SIZE / MOVEMENT_FRAMES;
        public static float GRID_STEP_SIZE = TILE_STEP_SIZE / 1.75f;
        // How many frames to wait for input before moving (allows for easier diagonal movement).
        public static int INPUT_DELAY_FRAMES = 2;

        public static readonly string DEFAULT_LOC_FILE = "Content/Localization/en_us.xml";

        public static bool VSync = true;
        public static bool IS_FULLSCREEN = false;
        public static int FPS = 60;
    }
}
