using System.Collections.Generic;

namespace OGP.Middleware
{
    public interface IChatClient
    {
        List<string> ClientsEndpoints { get; set; }
        void ActivateClients();
        void SendMsg(string message);
        void MsgToClient(string message);
    }
}
