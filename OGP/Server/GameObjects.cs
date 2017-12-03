namespace OGP.Server
{
    internal abstract class Mappable
    {
        private int x;
        private int y;

        public Mappable(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public int GetX()
        {
            return this.x;
        }

        public int GetY()
        {
            return this.y;
        }
    }

    internal class GamePlayer : Mappable
    {
        private string playerId;
        private int score;
        private bool alive;

        public GamePlayer(int x, int y, string playerId, int score, bool alive) : base(x, y)
        {
            this.playerId = playerId;
            this.score = score;
            this.alive = alive;
        }

        public string GetPlayerId()
        {
            return playerId;
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

    internal class GameGhost : Mappable
    {
        public GameGhost(int x, int y) : base(x, y)
        {
        }
    }

    internal class GameCoin : Mappable
    {
        public GameCoin(int x, int y) : base(x, y)
        {
        }
    }

    internal class GameServer
    {
        private string Url;

        public GameServer(string Url)
        {
            this.Url = Url;
        }

        public string GetUrl()
        {
            return Url;
        }
    }
}