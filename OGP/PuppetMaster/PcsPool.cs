using OGP.PCS;
using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace OGP.PuppetMaster
{
    internal class PcsPool
    {
        private static Dictionary<string, PcsManager> pcsManagers = new Dictionary<string, PcsManager>();
        private static Dictionary<string, string> pidToLink = new Dictionary<string, string>();
        private static TcpChannel channel = null;

        public static PcsManager GetByUrl(string url)
        {
            if (pcsManagers.TryGetValue(url, out PcsManager pcsManager))
            {
                return pcsManager;
            }

            if (channel == null)
            {
                channel = new TcpChannel();
                ChannelServices.RegisterChannel(channel, true);
            }

            pcsManager = (PcsManager)Activator.GetObject(typeof(PcsManager), url);

            pcsManagers.Add(url, pcsManager);

            return pcsManager;
        }

        public static PcsManager GetByPid(string pid)
        {
            if (pidToLink.TryGetValue(pid, out string url))
            {
                return GetByUrl(url);
            }

            return null;
        }

        internal static void LinkPid(string pid, string pcsUrl)
        {
            pidToLink.Remove(pid); // Prevent error for duplicate key
            // TODO: UnlinkPid after Crash command is executed
            pidToLink.Add(pid, pcsUrl);
        }

        internal static List<string> GetAllPids()
        {
            return new List<string>(pidToLink.Keys);
        }
    }
}