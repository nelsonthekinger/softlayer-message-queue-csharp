using System.Collections.Generic;
using System.Linq;

namespace SoftLayer.Messaging
{
    public class ServiceDatacenter
    {
        public string DataCenter { get; set; }
        public List<ServiceEndpointUrl> URLs { get; set; }

        public ServiceDatacenter()
        {
            URLs = new List<ServiceEndpointUrl>();
        }

        public string GetFirstUrl(bool usePrivateEndpoint = false)
        {
            ServiceEndpointUrl endpoint = URLs.First<ServiceEndpointUrl>(
                delegate(ServiceEndpointUrl tmpEndpoint)
                {
                    return tmpEndpoint.PrivateNetwork == usePrivateEndpoint;
                });

            if (endpoint == null) {
                throw new NoSuitableEndpointException("No suitable endpoint found for " + DataCenter);
            }

            return endpoint.URL;
        }
    }
}
