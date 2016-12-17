using Microsoft.Xna.Framework;

namespace Broken_Shadows
{
    public class GlobalDefines
    {
        // Graphics
        public static readonly int WindowWidth = 1216; // Default 896.
        public static readonly int WindowHeight = 832; // Default 832.
        public static readonly int MouseCursorSize = 32; // Default 32.
        public static bool VSync = true;  // Default true.
        public static bool IsFullscreen = false;  // Default false.
        public static readonly bool UseFpsCap = false; // Default false.
        public static readonly int MaxGameFps = 60; // Default 60.
        public static readonly int MaxFpsSamples = 100; // Default 100.
        public static readonly Color BackgroundColor = Color.Gray; // Default Gray.

        // Grid Setup
        public static readonly int MovementFrames = 8; // Default 8.
        public static readonly int TileSize = 64; // Default 64.
        public static readonly int PlayerSize = 52; // Default 52.
        public static readonly int TileStepSize = TileSize / MovementFrames;
        public static readonly float GridStepSize = TileStepSize / 1.75f; // Default factor of 1.75f.
        public static readonly int InputDelayFrames = 1; // Default 2. Used for diagonal movement input.

        public static readonly string DefaultLocFile = "Content/Localization/en_us.xml";

        // Game Tweaks
        public static readonly int StartLevel = 1; // Default 1.
    }
}
