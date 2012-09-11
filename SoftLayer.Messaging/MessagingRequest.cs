using System;
using System.Collections.Generic;
using RestSharp;

namespace SoftLayer.Messaging
{
    public class MessagingRequest : RestRequest
    {
        public Dictionary<int, Type> HttpStatusExceptionMap = new Dictionary<int, Type>();
        public HashSet<int> HttpStatusSuccessCodes = new HashSet<int>();
        public bool AutoAuthenticate = true;

        public bool IsPublicEndpoint
        {
            get { return Resource.ToLower().Trim('/') == "ping"; }
        }

        public bool IsAuthRequest
        {
            get { return Resource.ToLower().Trim('/') == "auth"; }
        }

        public MessagingRequest()
            : base()
        {
            this.RequestFormat = DataFormat.Json;
        }

        public MessagingRequest(string resource, Method method = RestSharp.Method.GET)
            : base(resource, method)
        {
            this.RequestFormat = DataFormat.Json;
        }
    }
}
