using System;
using static OGP.Server.ActionHandler;

namespace OGP.Server
{
    [Serializable]
    public enum Direction
    { UP, DOWN, LEFT, RIGHT, NONE };

    [Serializable]
    public enum GhostColor
    { Pink, Red, Yellow };

    [Serializable]
    public class Player
    {
        public string PlayerId { get; set; }
        public string Url { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Score { get; set; }
        public bool Alive { get; set; }
        public Direction Direction { get; set; }
        public int Width { get; internal set; }
        public int Height { get; internal set; }
    }

    [Serializable]
    public class Ghost
    {
        public GhostColor Color { get; internal set; }
        public int X { get; internal set; }
        public int Y { get; internal set; }
        public int Width { get; internal set; }
        public int Height { get; internal set; }
    }

    [Serializable]
    public class Coin
    {
        public int X { get; internal set; }
        public int Y { get; internal set; }
        public int Width { get; internal set; }
        public int Height { get; internal set; }
    }

    [Serializable]
    public class Wall
    {
        public int X { get; internal set; }
        public int Y { get; internal set; }
        public int Width { get; internal set; }
        public int Height { get; internal set; }
    }

    [Serializable]
    public class Server
    {
        public string Url { get; internal set; }
    }
    
    [Serializable]
    public class GameMovement
    {
        public Direction Direction { get; set; }
    }

    [Serializable]
    public class ClientSync
    {
        public string Pid { get; set; }
    }

    [Serializable]
    public class ChatMessage
    {
        public string Sender { get; set; }
        public string Message { get; set; }
    }
}