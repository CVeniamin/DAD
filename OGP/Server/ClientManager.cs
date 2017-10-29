using System;
using System.Threading;

namespace OGP.Server
{
    internal class ClientManager
    {
        private GameManager gameManager;
        private Uri baseUri;

        public ClientManager(GameManager gameManager, Uri baseUri)
        {
            this.gameManager = gameManager;
            this.baseUri = baseUri;
        }

        internal void Listen()
        {
            Console.WriteLine("[ClientManager]: Listening on {0}", this.baseUri.AbsoluteUri);

            Thread.Sleep(5000);

            Console.WriteLine("[ClientManager]: Received request for Random");
            // Search existing rooms
            Console.WriteLine("[ClientManager]: No spare rooms found");
            // Create new room
            string gameSessionId = gameManager.CreateSession("Random");
            Console.WriteLine("[ClientManager]: Created new room {0}", gameSessionId);
            // Send client message with gameSessionId and details to join

            Thread.Sleep(5000);

            Player player = new Player
            {
                PlayerId = "abcde"
            };
            gameManager.PlayerJoinedRoom(gameSessionId, player);

            Thread.Sleep(5000);

            gameManager.PlayerLeftRoom(gameSessionId, player);
        }

        internal void Exit()
        {
            Console.WriteLine("[ClientManager] Exiting...");
            Console.WriteLine("[ClientManager] Exited");
        }
    }
}