using System;

namespace OGP.Server
{
    internal class ClusterMember
    {
        public ServerDefinition serverDefinition;

        public ClusterMember(object something)
        {
            // Console.WriteLine("Cluter member said they support: " + String.Join(String.Empty, this.serverDefinition.SupportedGames.ToArray()));
        }
        
    }
}