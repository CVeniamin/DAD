using System;
using System.Threading.Tasks;

namespace OGP.Server
{
    internal class ClusterMember
    {
        private Uri controlUri;

        public ClusterMember(Uri controlUri)
        {
            this.controlUri = controlUri;
            Console.WriteLine("New ClusterMember with URI {0}", controlUri.AbsoluteUri);
        }

        internal Task<bool> isAlive()
        {
            return Task.FromResult<bool>(true);
        }
    }
}