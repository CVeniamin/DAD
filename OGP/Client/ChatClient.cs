using System;
using System.Collections.Generic;
using System.Threading;
using OGP.Middleware;

namespace OGP.Client
{
    internal class ChatClient : MarshalByRefObject, IChatClient
    {
        private delegate void AddMessageDelegate(string mensagem);

        public static MainFrame form;

        public void MsgToClient(string mensagem)
        {
            // thread-safe access to form
            form.Invoke(new AddMessageDelegate(form.AddMsg), mensagem);
        }
        private const string SERVICE_NAME = "/ChatClient";

        private List<string> messages;
        private string url;
        private List<string> clientsEndpoints;
        private List<IChatClient> clients;
        public List<IChatClient> Clients { get => clients; set => clients = value; }
        public List<string> ClientsEndpoints { get => clientsEndpoints; set => clientsEndpoints = value; }

        private string pid;
        public string Pid { get => pid; set => pid = value; }
        public string Url { get => url; set => url = value; }

        public ChatClient(MainFrame mf, string p, string u)
        {
            clients = new List<IChatClient>();
            clientsEndpoints = new List<string>();
            messages = new List<string>();
            form = mf;
            pid = p;
            url = u;
        }

        public void SendMsg(string mensagem)
        {
            messages.Add(mensagem);
            ThreadStart ts = new ThreadStart(this.BroadcastMessage);
            Thread t = new Thread(ts);
            t.Start();
        }

        private void BroadcastMessage()
        {
            string MsgToBcast;
            lock (this)
            {
                MsgToBcast = pid + " : " + messages[messages.Count - 1];
            }
            for (int i = 0; i < clients.Count; i++)
            {
                try
                {
                    ((ChatClient)clients[i]).MsgToClient(MsgToBcast);
                }
                catch (Exception e)
                {
                    MsgToClient(e.ToString());
                    clients.RemoveAt(i);
                }
            }
        }

        public void ActivateClients()
        {
            foreach (var url in clientsEndpoints)
            {
                clients.Add((IChatClient)Activator.GetObject(typeof(IChatClient), url + SERVICE_NAME));
            }
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }
    }
}
