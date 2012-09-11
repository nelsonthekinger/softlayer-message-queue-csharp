using System;
using System.Collections.Generic;
using RestSharp;
using SoftLayer.Messaging.Primitives;

namespace SoftLayer.Messaging
{
    public class MessagingApi
    {
        public static bool DebugRequests = false;

        private string accountId = string.Empty;
        private string userName = string.Empty;
        private string apiKey = string.Empty;
        private string apiToken = string.Empty;
        private string apiEndpoint = string.Empty;

        private bool autoAuthenticationLocked = false;

        public string ApiToken
        {
            get { return apiToken; }
            set { apiToken = value; }
        }

        public string ApiEndpoint
        {
            get { return apiEndpoint; }
            set { apiEndpoint = value; }
        }

        public bool IsPendingAuthentication
        {
            get { return apiToken.Length == 0; }
        }


        public MessagingApi(string accountId, string userName, string apiKey, string apiEndpoint)
        {
            this.accountId = accountId;
            this.userName = userName;
            this.apiKey = apiKey;
            this.apiEndpoint = apiEndpoint;
        }

        private string getBaseUrl(bool accountLevel = true)
        {
            if (accountLevel) {
                return apiEndpoint + accountId;
            }

            return apiEndpoint;
        }

        public string Execute(MessagingRequest request)
        {
            RestClient client = new RestClient();
            client.BaseUrl = getBaseUrl(false);

            if (DebugRequests) {
                Console.WriteLine(request.Method + " " + client.BaseUrl + "/" + request.Resource);
            }

            var response = client.Execute(request);

            switch (response.StatusCode) {
                /* 0 */
                case 0:
                    throw new ServerUnreachableException();

                /* 400 */
                case System.Net.HttpStatusCode.BadRequest:
                    throw new BadRequestException(response.Content);

                /* 401 */
                case System.Net.HttpStatusCode.Unauthorized:
                    throw new UnauthorizedException(response.Content);

                /* 500 */
                case System.Net.HttpStatusCode.InternalServerError:
                    throw new ServerErrorException();

                /* 502 */
                case System.Net.HttpStatusCode.BadGateway:
                    throw new ServerErrorException();

                /* 503 */
                case System.Net.HttpStatusCode.ServiceUnavailable:
                    throw new ServiceUnavailableException(response.Content);
            }

            if (!request.HttpStatusSuccessCodes.Contains((int) response.StatusCode)) {
                throw new UnexpectedResponseException(response.StatusCode.ToString());
            }

            return response.Content;
        }

        public T Execute<T>(MessagingRequest request) where T : new()
        {
            RestClient client = new RestClient();
            client.BaseUrl = getBaseUrl(false);

            if (request.AutoAuthenticate && IsPendingAuthentication && !autoAuthenticationLocked && !request.IsAuthRequest) {
                autoAuthenticationLocked = true;

                try {
                    MessagingRequest authRequest = new MessagingRequest("auth", Method.POST);
                    authRequest.HttpStatusSuccessCodes.Add(200);
                    authRequest.AutoAuthenticate = false;
                    AuthResponse authResponse = Execute<AuthResponse>(authRequest);
                    ApiToken = authResponse.token;
                }
                catch (Exception e) {
                    autoAuthenticationLocked = false;
                    throw e;
                }
            }

            if (!request.IsPublicEndpoint) {
                client.BaseUrl = getBaseUrl();

                if (request.IsAuthRequest) {
                    request.AddHeader("X-Auth-User", userName);
                    request.AddHeader("X-Auth-Key", apiKey);
                }
                else if (!IsPendingAuthentication) {
                    request.AddHeader("X-Auth-Token", apiToken);
                }
            }

            if (DebugRequests) {
                Console.WriteLine(request.Method + " " + client.BaseUrl + "/" + request.Resource);
            }

            var response = client.Execute<T>(request);

            if (DebugRequests) {
                Console.WriteLine("HTTP " + response.StatusCode.ToString("d") + " " + response.StatusCode);
                Console.WriteLine("<- " + response.Content);
            }

            switch (response.StatusCode) {
                /* 0 */
                case 0:
                    throw new ServerUnreachableException();

                /* 400 */
                case System.Net.HttpStatusCode.BadRequest:
                    throw new BadRequestException(response.Content);

                /* 401 */
                case System.Net.HttpStatusCode.Unauthorized:
                    if (ApiToken.Length > 0) {
                        /** Problem with API token; likely expired */
                        throw new TokenInvalidOrExpiredException(response.Content);
                    }
                    else {
                        throw new UnauthorizedException(response.Content);
                    }

                /* 503 */
                case System.Net.HttpStatusCode.ServiceUnavailable:
                    throw new ServiceUnavailableException();
            }

            foreach (KeyValuePair<int, Type> tmpEntry in request.HttpStatusExceptionMap) {
                if ((int) response.StatusCode == tmpEntry.Key) {
                    throw (Exception) Activator.CreateInstance(
                        tmpEntry.Value,
                        new object[] { response.Content });
                }
            }

            if (!request.HttpStatusSuccessCodes.Contains((int) response.StatusCode)) {
                throw new UnexpectedResponseException(String.Format("HTTP/{0} {1}: {2}",
                    ((int) response.StatusCode).ToString(),
                    response.StatusDescription,
                    response.Content));
            }

            return response.Data;
        }
    }
}
