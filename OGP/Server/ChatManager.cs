using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
