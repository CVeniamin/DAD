using System;

namespace OGP.Server
{
    internal class ClientManager
    {
        private GameManager gameManager;
        private string serviceUrl;

        public ClientManager(GameManager gameManager, string serviceUrl)
        {
            this.gameManager = gameManager;
            this.serviceUrl = serviceUrl;
        }

        internal void listen()
        {
            Console.WriteLine("[ClientManager]: Listening on {0}", this.serviceUrl);
        }
    }
}