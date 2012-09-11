using NUnit.Framework;
using SoftLayer.Messaging;

namespace UnitTests
{
    [TestFixture]
    public class PingTests : MessagingTest
    {
        [TestFixtureSetUp]
        protected override void Init()
        {
            base.Init();
            validClient.Authenticate();
        }

        [Test]
        public void PingSuccess()
        {
            Assert.IsTrue(validClient.Ping());
        }

        [Test]
        public void PingFailure()
        {
            invalidClient.CustomEndpoint = "http://queue.localhost.localdomain/v1/";
            Assert.Throws<ServerUnreachableException>(
                delegate { invalidClient.Ping(); });
        }
    }
}
