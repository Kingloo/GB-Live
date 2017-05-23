using System;
using GBLive.WPF;
using NUnit.Framework;

namespace GBLive.Tests
{
    [TestFixture]
    public class GBSettingsTests
    {
        private static Uri google = new Uri("http://google.com");
        private static Uri bing = new Uri("http://bing.com");
        
        [Test]
        public void GBSettings_Ctor_ParamsAreCorrectlyAssignedToProps()
        {
            TimeSpan interval = TimeSpan.FromMinutes(5);

            var gbs = new GBSettings(interval);
            
            bool UpdateIntervalSetCorrectly = gbs.UpdateInterval.Equals(interval);
            
            Assert.IsTrue(UpdateIntervalSetCorrectly);
        }

        [Test]
        public void GBSettings_Ctor_IsIntervalMinimumRespected()
        {
            var intervalLessThan3Minutes = TimeSpan.FromMilliseconds(1);

            var gbs = new GBSettings(intervalLessThan3Minutes);

            bool wasItRespected = gbs.UpdateInterval > intervalLessThan3Minutes;

            Assert.IsTrue(wasItRespected);
        }
    }
}
