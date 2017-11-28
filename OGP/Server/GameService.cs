using System;
using System.IO;

namespace OGP.Server
{
    public class GameService : MarshalByRefObject, IGameService
    {
        private System.Xml.Serialization.XmlSerializer serializer =
             new System.Xml.Serialization.XmlSerializer(typeof(Game));

        public bool AddGame(Game g)
        {
            TextWriter tw = new StreamWriter(g.GameID + ".Game");
            serializer.Serialize(tw, g);
            tw.Close();
            return true;
        }

        public Game GetGame(int gameID)
        {
            TextReader tr = null;
            if (!File.Exists(gameID + ".Game"))
            {
                throw new GameNotFoundException("");
            }
            try
            {
                tr = new StreamReader(gameID + ".Game");
                Game Game = (Game)serializer.Deserialize(tr);
                return Game;
            }
            catch (Exception e)
            {
                throw new GameNotFoundException(e.Message);
            }
            finally
            {
                tr.Close();
            }
        }

        public bool DeleteGame(int gameID)
        {
            if (File.Exists(gameID + ".Game"))
            {
                File.Delete(gameID + ".Game");
            }
            else
            {
                throw new GameNotFoundException("Game Not found!");
            }
            return true;
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }
    }
}