using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OGP.Server
{
    abstract class Mappable
    {
        private int x;
        private int y;

        public Mappable(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public int GetX() {
            return this.x;
        }

        public int GetY()
        {
            return this.y;
        }
    }

    class GamePlayer : Mappable
    {
        private int score;
        private bool alive;

        public GamePlayer(int x, int y, int score, bool alive) : base(x, y)
        {
            this.score = score;
            this.alive = alive;
        }

        public int GetScore()
        {
            return score;
        }

        public bool IsAlive()
        {
            return alive;
        }
    }

    class GameGhost: Mappable
    {
        public GameGhost(int x, int y) : base(x, y)
        {

        }
    }

    class GameCoin : Mappable
    {
        public GameCoin(int x, int y) : base(x, y)
        {

        }
    }

    class GameServer : Mappable
    {
        private string Url;

        public GameServer(int x, int y, string Url) : base(x, y)
        {
            this.Url = Url;
        }

        public string GetUrl()
        {
            return Url;
        }
    }
}
