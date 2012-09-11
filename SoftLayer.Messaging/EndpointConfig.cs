using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SoftLayer.Messaging
{
    public class EndpointConfig
    {
        const string FileName = "SoftLayer.Messaging.Endpoints.xml";
        private List<ServiceDatacenter> endpoints;

        public string FilePath
        {
            get
            {
                return (new DirectoryInfo(this.GetType().Assembly.Location))
                    .Parent.FullName + @"\" + FileName;
            }
        }

        public EndpointConfig()
        {
            endpoints = new List<ServiceDatacenter>();
        }

        public bool Load()
        {
            if (!File.Exists(FilePath)) {
                throw new FileNotFoundException("Cannot find endpoints XML file at " + FilePath);
            }

            XElement xmlRoot = XElement.Load(FilePath);
            IEnumerable<XElement> xmlEndpointNodes =
                from tmpNode in xmlRoot.Elements()
                select tmpNode;

            // Iterate through the Endpoint nodes.
            foreach (XElement tmpNode in xmlEndpointNodes) {
                IEnumerable<XElement> xmlUrlNodes =
                    from subel in xmlEndpointNodes.Elements()
                    select subel;

                // Search for the datacenter attribute and skip this node if none is found.
                var dcAttribute = tmpNode.Attributes().First<XAttribute>(
                    delegate(XAttribute x)
                    {
                        return "datacenter" == x.Name.LocalName.ToLower();
                    });

                if (dcAttribute == null) {
                    continue;
                }

                ServiceDatacenter endpoint = new ServiceDatacenter();
                endpoint.DataCenter = dcAttribute.Value.ToUpper();

                // Iterate through the URL nodes
                foreach (XElement tmpUrlNode in xmlUrlNodes) {
                    // Search for the type attribute instead of directly referencing it. Thanks, LINQ!
                    var typeAttribute = tmpUrlNode.Attributes().First<XAttribute>(
                        delegate(XAttribute x)
                        {
                            return "type" == x.Name.LocalName.ToLower();
                        });

                    if (typeAttribute != null) {
                        // Only add an endpoint URL if it has a type.
                        ServiceEndpointUrl url = new ServiceEndpointUrl();
                        url.PublicNetwork = ("public" == typeAttribute.Value.ToLower());
                        url.URL = tmpUrlNode.Value;

                        endpoint.URLs.Add(url);
                    }
                }

                endpoints.Add(endpoint);
            }

            return (endpoints.Count > 0);
        }

        public string GetUrlForDatacenter(string dataCenter, bool usePrivateEndpoint = false)
        {
            ServiceDatacenter endpoint = null;

            try {
                endpoint = endpoints.First<ServiceDatacenter>(
                    delegate(ServiceDatacenter e)
                    {
                        return dataCenter.ToLower() == e.DataCenter.ToLower();
                    });
            }
            catch (InvalidOperationException e) {
                throw new InvalidDatacenterException(dataCenter + " is not a valid or defined datacenter.", e);
            }

            return endpoint.GetFirstUrl(usePrivateEndpoint);
        }
    }
}
