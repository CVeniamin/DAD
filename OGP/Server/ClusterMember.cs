using System;

namespace OGP.Server
{
    internal class ClusterMember
    {
        private ClusterService.Client thriftClient;
        public ServerDefinition serverDefinition;

        public ClusterMember(ClusterService.Client thriftClient)
        {
            this.thriftClient = thriftClient;
            this.serverDefinition = thriftClient.GetDefinition();
            Console.WriteLine("Cluter member said they support: " + String.Join(String.Empty, this.serverDefinition.SupportedGames.ToArray()));
        }
        
    }
}