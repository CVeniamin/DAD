namespace OGP.Server
{
    public class Player
    {
        public string PlayerId { get; internal set; }
        public int X { get; internal set; }
        public int Y { get; internal set; }
        public int Score { get; internal set; }
        public bool Alive { get; internal set; }
    }

    public class Ghost
    {
        public GhostType Type { get; internal set; }
        public int X { get; internal set; }
        public int Y { get; internal set; }
    }

    public class Coin
    {
        public int X { get; internal set; }
        public int Y { get; internal set; }
    }

    public class Wall
    {
        public int X { get; internal set; }
        public int SizeX { get; set; }
        public int Y { get; internal set; }
        public int SizeY { get; set; }
    }

    public class Server
    {
        public string Url { get; internal set; }
    }
}