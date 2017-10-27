using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace OGP.Server
{
    internal class XmlHttpServer
    {
        private Thread _serverThread;
        private HttpListener _listener;
        private Uri listenBaseUri;
        private Dictionary<string, byte[]> xmls;

        public XmlHttpServer(Uri listenBaseUri, Dictionary<string, string> xmlDocs)
        {
            this.listenBaseUri = listenBaseUri;
            this.xmls = new Dictionary<string, byte[]>();

            foreach (KeyValuePair<string, string> entry in xmlDocs)
            {
                xmls.Add(entry.Key, Encoding.UTF8.GetBytes(entry.Value));
            }
            
            _serverThread = new Thread(this.Listen);
            _serverThread.Start();
        }

        public void Stop()
        {
            _serverThread.Abort();
            _listener.Stop();
        }

        private void Listen()
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add(string.Format("http://{0}:{1}/", listenBaseUri.Host, listenBaseUri.Port));
            _listener.Start();
            while (true)
            {
                try
                {
                    HttpListenerContext context = _listener.GetContext();
                    Process(context);
                }
                catch (Exception)
                {
                }
            }
        }

        private void Process(HttpListenerContext context)
        {
            string path = context.Request.Url.AbsolutePath.Substring(1);

            byte[] dd;
            if (xmls.TryGetValue(path, out dd))
            {
                try
                {
                    context.Response.ContentType = "text/xml";
                    context.Response.ContentLength64 = dd.Length;
                    context.Response.AddHeader("Date", DateTime.Now.ToString("r"));
                    context.Response.AddHeader("Last-Modified", DateTime.Now.ToString("r"));
                    context.Response.OutputStream.Write(dd, 0, dd.Length);
                    context.Response.StatusCode = (int)HttpStatusCode.OK;
                    context.Response.OutputStream.Flush();
                }
                catch (Exception)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                }
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            }

            context.Response.OutputStream.Close();
        }
    }
}