namespace OGP.Server
{
    internal class Player
    {
        public string PlayerId { get; internal set; }
        public int X { get; internal set; }
        public int Y { get; internal set; }
        public int Score { get; internal set; }
        public bool Alive { get; internal set; }
    }

    internal class Ghost
    {
        public int X { get; internal set; }
        public int Y { get; internal set; }
    }

    internal class Coin
    {
        public int X { get; internal set; }
        public int Y { get; internal set; }
    }

    internal class Server
    {
        public string Url { get; internal set; }
    }
}