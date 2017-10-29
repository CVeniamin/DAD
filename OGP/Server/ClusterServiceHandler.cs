namespace OGP.Server
{
    class ClusterServiceHandler : ClusterService.Iface
    {
        private ClusterManager clusterManager;
        private ServerDefinition serverDefinition;

        public ClusterServiceHandler(ClusterManager clusterManager, ServerDefinition serverDefinition)
        {
            this.clusterManager = clusterManager;
            this.serverDefinition = serverDefinition;
        }

        public ServerDefinition GetDefinition()
        {
            return this.serverDefinition;
        }
    }
}
