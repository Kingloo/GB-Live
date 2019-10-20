using System;
using NUnit.Framework;

namespace GBLive.Tests.GiantBombTests.SettingsTests
{
    public class UserAgentTests
    {
        [Test]
        public void UserAgent_MustNotBeNull()
        {
            Assert.IsFalse(String.IsNullOrWhiteSpace(GiantBomb.Settings.UserAgent));
        }
    }
}