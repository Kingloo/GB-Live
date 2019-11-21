using System;
using NUnit.Framework;

namespace GBLive.Tests.GiantBombTests.SettingsTests
{
    public class UpdateIntervalTests
    {
        private readonly TimeSpan updateIntervalMinimum = TimeSpan.FromMinutes(1d);
        private readonly TimeSpan updateIntervalMaximum = TimeSpan.FromMinutes(10d);

        [Test]
        public void UpdateInterval_NotTooHigh()
        {
            Assert.IsTrue(GiantBomb.Settings.UpdateInterval <= updateIntervalMaximum);
        }

        [Test]
        public void UpdateInterval_NotTooLow()
        {
            Assert.IsTrue(GiantBomb.Settings.UpdateInterval >= updateIntervalMinimum);
        }
    }
}