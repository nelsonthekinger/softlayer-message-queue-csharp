using SoftLayer.Messaging;

namespace UnitTests
{
    public static class TestClientFactory
    {
        public static MessagingClient CreateValid()
        {
            return new MessagingClient(
                Credentials.Valid.AccountId,
                Credentials.Valid.UserName,
                Credentials.Valid.ApiKey,
                Credentials.Valid.DataCenter);
        }

        public static MessagingClient CreateInvalid()
        {
            return new MessagingClient(
                Credentials.Invalid.AccountId,
                Credentials.Invalid.UserName,
                Credentials.Invalid.ApiKey,
                Credentials.Invalid.DataCenter);
        }
    }

}
