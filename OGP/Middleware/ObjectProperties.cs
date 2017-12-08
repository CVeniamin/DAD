namespace OGP.Middleware
{
    public class ObjectDimensions
    {
        public const int COIN_HEIGHT = 15;
        public const int COIN_WIDTH = 15;

        public const int GHOST_HEIGHT = 35;
        public const int GHOST_WIDTH = 35;

        public const int PLAYER_HEIGHT = 35;
        public const int PLAYER_WIDTH = 35;

        public const int BOARD_WIDTH = 320;
        public const int BOARD_HEIGHT = 320;
    }

    public class GhostConstants
    {
        public int STARTING_X { get; set; }
        public int STARTING_Y { get; set; }
        public int VELOCITY_X { get; set; }
        public int VELOCITY_Y { get; set; }
    }

    public class GameConstants
    {
        public const int PLAYER_SPEED = 5;

        public static readonly GhostConstants PINK_GHOST = new GhostConstants
        {
            STARTING_X = 210,
            STARTING_Y = 120,
            VELOCITY_X = 1,
            VELOCITY_Y = 2
        };

        public static readonly GhostConstants YELLOW_GHOST = new GhostConstants
        {
            STARTING_X = 200,
            STARTING_Y = 235,
            VELOCITY_X = 5,
            VELOCITY_Y = 0
        };

        public static readonly GhostConstants RED_GHOST = new GhostConstants
        {
            STARTING_X = 240,
            STARTING_Y = 90,
            VELOCITY_X = 5,
            VELOCITY_Y = 0
        };
    }
}