using System;
using System.Collections.Generic;
using OGP.Middleware;

namespace OGP.Server
{
    internal class ChatManager : MarshalByRefObject, IChatManager
    {

        private List<string> clientsEndpoints;
        public List<string> ClientsEndpoints { get => clientsEndpoints; set => clientsEndpoints = value; }

        private bool gameStarted;
        public bool GameStarted { get => gameStarted; set => gameStarted = value; }

        public ChatManager()
        {
            gameStarted = false;
            clientsEndpoints = new List<string>();
        }

        public void RegisterClient(string url)
        {
            Console.WriteLine("New client listening at " + url);
            ClientsEndpoints.Add(url);
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }

        public List<string> GetClients()
        {
            return ClientsEndpoints;
        }
    }
}