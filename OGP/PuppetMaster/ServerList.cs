using System.Collections.Generic;

namespace OGP.PuppetMaster
{
    public class ServerList
    {
        private static List<string> servers = new List<string>();

        public static void AddServer(string url)
        {
            servers.Add(url);
        }

        public static List<string> Servers { get => servers; }
    }
}