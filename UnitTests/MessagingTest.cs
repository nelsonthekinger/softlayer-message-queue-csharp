using System;
using SoftLayer.Messaging;


namespace UnitTests
{
    public class MessagingTest
    {
        protected MessagingClient validClient;
        protected MessagingClient invalidClient;

        protected virtual void Init()
        {
            validClient = TestClientFactory.CreateValid();
            invalidClient = TestClientFactory.CreateInvalid();
        }

        protected void T(string message) {
            Console.WriteLine("[" + DateTime.Now.ToLongTimeString() + "] " + message);
        }
        protected void TC(string message)
        {
            Console.Write(message);
        }
    }
}
