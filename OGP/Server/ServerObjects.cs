using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OGP.Server
{
    class Player
    {
        public string PlayerId { get; internal set; }
        public int X { get; internal set; }
        public int Y { get; internal set; }
        public int Score { get; internal set; }
        public bool Alive { get; internal set; }
    }

    class Ghost
    {
        public int X { get; internal set; }
        public int Y { get; internal set; }
    }

    class Coin
    {
        public int X { get; internal set; }
        public int Y { get; internal set; }
    }

    class Server
    {
        public string Url { get; internal set; }
    }
}
