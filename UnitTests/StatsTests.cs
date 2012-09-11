using NUnit.Framework;
using SoftLayer.Messaging;

namespace UnitTests
{
    [TestFixture]
    public class StatsTests : MessagingTest
    {
        [TestFixtureSetUp]
        public new void Init()
        {
            base.Init();
            validClient.Authenticate();
        }

        [Test]
        public void GetHourlyStats()
        {
            MessagingStats stats = validClient.GetStatsForHour();
            Assert.NotNull(stats);
        }

        [Test]
        public void GetDailyStats()
        {
            MessagingStats stats = validClient.GetStatsForDay();
            Assert.NotNull(stats);
        }

        [Test]
        public void GetWeeklyStats()
        {
            MessagingStats stats = validClient.GetStatsForWeek();
            Assert.NotNull(stats);
        }

        [Test]
        public void GetMonthlyStats()
        {
            MessagingStats stats = validClient.GetStatsForMonth();
            Assert.NotNull(stats);
        }
    }
}
