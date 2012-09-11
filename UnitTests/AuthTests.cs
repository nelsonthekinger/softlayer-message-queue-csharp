using NUnit.Framework;
using SoftLayer.Messaging;

namespace UnitTests
{
    [TestFixture]
    public class AuthTests : MessagingTest
    {
        [TestFixtureSetUp]
        public new void Init()
        {
            base.Init();
        }

        [Test]
        public void AuthSuccess()
        {
            Assert.IsTrue(validClient.Authenticate());
        }

        [Test]
        public void AuthFailure()
        {
            Assert.Throws<UnauthorizedException>(
                delegate { invalidClient.Authenticate(); });
        }
    }


}
