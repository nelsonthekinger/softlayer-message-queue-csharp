
namespace SoftLayer.Messaging
{
    public class ServiceEndpointUrl
    {
        public string URL { get; set; }
        public bool PublicNetwork { get; set; }
        public bool PrivateNetwork
        {
            get { return !PublicNetwork; }
            set { PublicNetwork = !value; }
        }
    }
}
