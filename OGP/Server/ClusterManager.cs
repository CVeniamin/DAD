using System;
using System.Collections.Generic;
using Rssdp;

namespace OGP.Server
{
    internal class ClusterManager
    {
        private XmlHttpServer xmlHttpServer;
        private SsdpDevicePublisher _Publisher;
        private SsdpDeviceLocator _DeviceLocator;
        private SsdpRootDevice deviceDefinition;
        private SsdpService deviceService;

        private string clusterId;
        private string serverId;
        private Uri serviceUri;

        private string descriptionLocation;

        private Dictionary<string, ClusterMember> servers;

        public ClusterManager(string clusterId, string serverId, Uri serviceUri)
        {
            this.clusterId = clusterId;
            this.serverId = serverId;
            this.serviceUri = serviceUri;

            servers = new Dictionary<string, ClusterMember>();

            PublishDevice();
            Console.WriteLine("[ClusterManager]: Joining cluster {0} as {1}", this.clusterId, this.serverId);

            BeginSearch();
        }

        public void PublishDevice()
        {
            string ddHost = this.serviceUri.Host;
            if (ddHost.Equals("127.0.0.1"))
            {
                ddHost = "localhost";
            }

            int ddPort = this.serviceUri.Port + 1;

            string ddPath = "dd.xml";

            this.descriptionLocation = string.Format("http://{0}:{1}/{2}", ddHost, ddPort, ddPath);
            Uri ddUri = new Uri(this.descriptionLocation);

            this.deviceDefinition = new SsdpRootDevice
            {
                CacheLifetime = TimeSpan.FromMinutes(5),
                Location = ddUri,
                DeviceTypeNamespace = "OGP",
                DeviceType = "Server",
                Manufacturer = "DAD",
                Uuid = this.serverId
            };
            
            this.deviceService = new SsdpService
            {
                Uuid = Guid.NewGuid().ToString(),
                ServiceType = "ServerClustering",
                ServiceTypeNamespace = "OGP",
                ControlUrl = serviceUri,
                ScpdUrl = new Uri("dd.xml", UriKind.Relative)
            };
            this.deviceDefinition.AddService(deviceService);

            Dictionary<string, string> xmlDocs = new Dictionary<string, string>()
            {
                { ddPath, this.deviceDefinition.ToDescriptionDocument()}
            };
            this.xmlHttpServer = new XmlHttpServer(ddUri, xmlDocs);
            
            _Publisher = new SsdpDevicePublisher();
            _Publisher.AddDevice(this.deviceDefinition);
        }
        
        public void BeginSearch()
        {
            _DeviceLocator = new SsdpDeviceLocator
            {
                NotificationFilter = "urn:OGP:device:Server:1"
            };
            _DeviceLocator.DeviceAvailable += DeviceLocator_DeviceAvailable;
            _DeviceLocator.StartListeningForNotifications();
            _DeviceLocator.SearchAsync();
        }
        
        async void DeviceLocator_DeviceAvailable(object sender, DeviceAvailableEventArgs e)
        {
            if (e.IsNewlyDiscovered && !e.DiscoveredDevice.DescriptionLocation.ToString().Equals(this.descriptionLocation))
            {
                SsdpDevice fullDevice = await e.DiscoveredDevice.GetDeviceInfo();

                Uri controlUri = null;
                foreach (SsdpService service in fullDevice.Services)
                {
                    if (!service.ServiceType.Equals("ServerClustering") || !service.ServiceTypeNamespace.Equals("OGP"))
                    {
                        continue;
                    }

                    controlUri = service.ControlUrl;
                }

                if (controlUri != null)
                {
                    ClusterMember newPeer = new ClusterMember(controlUri);
                    if (await newPeer.isAlive())
                    {
                        servers.Add(fullDevice.Uuid, newPeer);
                        Console.WriteLine("[ClusterManager]: Adding cluster peer {0}", fullDevice.Uuid);
                    }
                }
            }
        }
    }
}