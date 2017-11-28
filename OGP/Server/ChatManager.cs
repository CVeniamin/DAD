using System.Collections.Generic;

namespace OGP.Server
{
    public interface IChatManager
    {
        IChatClient RegisterClient(string url);

        List<IChatClient> getClients();
    }

    public interface IChatClient
    {
        void SendMsg(string message);

        void MsgToClient(string message);
    }
}