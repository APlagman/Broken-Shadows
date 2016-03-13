namespace Broken_Shadows
{
    public class GlobalDefines
    {
        public static int WindowWidth = 896;
        public static int WindowHeight = 832;
        public static int MouseCursorSize = 32;

        public static int MovementFrames = 8;
        public static float TileSize = 64;
        public static float PlayerSize = 52;
        public static float PlayerOffset = (TileSize - PlayerSize) / 2;
        public static int GridSize = 11;
        public static float TileStepSize = TileSize / MovementFrames;
        public static float GridStepSize = TileStepSize / 1.75f;
        // How many frames to wait for input before moving (allows for easier diagonal movement).
        public static int InputDelayFrames = 2;

        public static readonly string DefaultLocFile = "Content/Localization/en_us.xml";

        public static bool VSync = true;
        public static bool IsFullscreen = false;
        public static int Fps = 60;
        public static int MaxFpsSamples = 100;
    }
}
