using System;
using System.Collections.Generic;

namespace OGP.Middleware
{
    public interface IChatManager
    {
        bool GameStarted { get; set; }
        void RegisterClient(string url);
        List<string> GetClients();
    }
}