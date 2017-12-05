﻿using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace OGP.Server
{
    [Serializable]
    public abstract class Mappable
    {
        private int x;
        private int y;

        public Mappable(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public int X { get => x; set => x = value; }
        public int Y { get => y; set => y = value; }
    }

    [Serializable]
    public class GamePlayer : Mappable
    {
        private string playerId;
        private int score;
        private bool alive;

        public GamePlayer(int x, int y, string playerId, int score, bool alive):base(x,y)
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

    public enum GhostType
    { Pink, Red, Yellow };

    [Serializable]
    public class GameGhost : Mappable
    {
        private GhostType type;

        public GameGhost(int x, int y, GhostType t) : base(x, y)
        {
            this.type = t;
        }

        public GhostType Type { get => type; set => type = value; }
    }

    [Serializable]
    public class GameCoin : Mappable
    {
        public GameCoin(int x, int y) : base(x, y)
        {
        }
    }

    [Serializable]
    public class GameServer
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