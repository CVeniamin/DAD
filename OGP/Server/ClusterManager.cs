using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Rssdp;
using Thrift.Protocol;
using Thrift.Server;
using Thrift.Transport;

namespace OGP.Server
{
    internal class ClusterManager
    {
        private XmlHttpServer clusterHttpServer;
        private SsdpDevicePublisher devicePublisher;
        private SsdpDeviceLocator deviceLocator;
        private SsdpRootDevice deviceDefinition;
        private SsdpService deviceService;

        private string clusterId;
        private string serverId;
        private Uri baseUri;

        private string ddLocation;

        private Dictionary<string, ClusterMember> clusterMembers;
        private Thread thriftServerThread;
        private TThreadPoolServer thriftServer;

        public ClusterManager(string clusterId, string serverId, Uri baseUri, ServerDefinition serverDefinition)
        {
            this.clusterId = clusterId;
            this.serverId = serverId;
            this.baseUri = baseUri;

            clusterMembers = new Dictionary<string, ClusterMember>();

            PublishDevice();
            Console.WriteLine("[ClusterManager]: Joining cluster {0} as {1}", this.clusterId, this.serverId);

            BeginSearch();
            Console.WriteLine("[ClusterManager]: Searching for other cluster nodes...");

            var clusterServiceHandler = new ClusterServiceHandler(this, serverDefinition);
            var clusterServiceProcessor = new ClusterService.Processor(clusterServiceHandler);
            
            TServerTransport thriftTransport = new TServerSocket(new TcpListener(Dns.GetHostAddresses(baseUri.Host)[0], baseUri.Port + 2), 5000);
            this.thriftServer = new TThreadPoolServer(clusterServiceProcessor, thriftTransport);

            this.thriftServerThread = new Thread(this.thriftServer.Serve);
            this.thriftServerThread.Start();
        }

        internal void Exit()
        {
            Console.WriteLine("[ClusterManager] Exiting...");

            Console.WriteLine("[ClusterManager] Unpublishing server");
            devicePublisher.RemoveDevice(this.deviceDefinition);

            Console.WriteLine("[ClusterManager] Stopping cluster service");
            this.thriftServer.Stop();
            thriftServerThread.Join();

            Console.WriteLine("[ClusterManager] Exited");
        }

        public string ClusterId => clusterId;
        public string ServerId => serverId;

        public void ClusterBroadcast(string message)
        {

        }

        public void ClusterUnicast(string uuid, string message)
        {

        }

        private void PublishDevice()
        {
            string ddHost = this.baseUri.Host;
            if (ddHost.Equals("127.0.0.1"))
            {
                ddHost = "localhost";
            }
            int ddPort = this.baseUri.Port + 1;
            string ddPath = "dd.xml";
            this.ddLocation = string.Format("http://{0}:{1}/{2}", ddHost, ddPort, ddPath);
            Uri ddUri = new Uri(this.ddLocation);

            this.deviceDefinition = new SsdpRootDevice
            {
                CacheLifetime = TimeSpan.FromMinutes(5),
                Location = ddUri,
                DeviceTypeNamespace = "OGP",
                ModelName = "OGP Server",
                FriendlyName = string.Format("OGP Server {0}", this.serverId),
                DeviceType = "Server",
                Manufacturer = "DAD",
                Uuid = this.serverId
            };
            
            string thriftHost = this.baseUri.Host;
            int thriftPort = this.baseUri.Port + 2;
            Uri thriftUri = new Uri(string.Format("tcp://{0}:{1}", thriftHost, thriftPort));

            this.deviceService = new SsdpService
            {
                Uuid = Guid.NewGuid().ToString(),
                ServiceType = "ServerClustering",
                ServiceTypeNamespace = "OGP",
                ControlUrl = thriftUri,
                ScpdUrl = new Uri(ddPath, UriKind.Relative)
            };
            this.deviceDefinition.AddService(deviceService);

            Dictionary<string, string> xmlDocs = new Dictionary<string, string>()
            {
                { ddPath, this.deviceDefinition.ToDescriptionDocument()}
            };
            this.clusterHttpServer = new XmlHttpServer(ddUri, xmlDocs);
            
            devicePublisher = new SsdpDevicePublisher();
            devicePublisher.AddDevice(this.deviceDefinition);
        }
        
        private void BeginSearch()
        {
            deviceLocator = new SsdpDeviceLocator
            {
                NotificationFilter = "urn:OGP:device:Server:1"
            };
            deviceLocator.DeviceAvailable += DeviceLocator_DeviceAvailable;
            deviceLocator.StartListeningForNotifications();
            deviceLocator.SearchAsync();
        }
        
        private async void DeviceLocator_DeviceAvailable(object sender, DeviceAvailableEventArgs e)
        {
            if (!e.IsNewlyDiscovered || e.DiscoveredDevice.DescriptionLocation.ToString().Equals(this.ddLocation))
            {
                return;
            }
            SsdpDevice fullDevice = await e.DiscoveredDevice.GetDeviceInfo();

            Uri controlUri = null;
            foreach (SsdpService service in fullDevice.Services)
            {
                if (!service.ServiceType.Equals("ServerClustering") || !service.ServiceTypeNamespace.Equals("OGP"))
                {
                    continue;
                }

                controlUri = service.ControlUrl;
                break;
            }

            if (controlUri != null)
            {
                AddClusterMember(fullDevice.Uuid, controlUri);
            }
        }

        private void AddClusterMember(string uuid, Uri controlUri)
        {
            ClusterService.Client thriftClient = null;
            try
            {
                var transport = new TSocket(controlUri.Host, controlUri.Port, 5000);
                thriftClient = new ClusterService.Client(new TBinaryProtocol(transport));
                transport.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                if (thriftClient != null)
                {
                    ClusterMember clusterMember = new ClusterMember(thriftClient);
                    this.clusterMembers.Add(uuid, clusterMember);
                    Console.WriteLine("[ClusterManager]: Adding cluster peer {0}", uuid);
                }
            }
        }
    }
}