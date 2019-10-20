using System;
using NUnit.Framework;

namespace GBLive.Tests.GiantBombTests.SettingsTests
{
    public class LinkTests
    {
        private const string secureScheme = "https";
        private const int securePort = 443;

        
        [Test]
        public void Home_IsHttps()
        {
            Assert.IsTrue(IsHttps(GiantBomb.Settings.Home));
        }

        [Test]
        public void Chat_IsHttps()
        {
            Assert.IsTrue(IsHttps(GiantBomb.Settings.Chat));
        }

        [Test]
        public void Upcoming_IsHttps()
        {
            Assert.IsTrue(IsHttps(GiantBomb.Settings.Upcoming));
        }

        [Test]
        public void FallbackImage_IsHttps()
        {
            Assert.IsTrue(IsHttps(GiantBomb.Settings.FallbackImage));
        }

        private static bool IsHttps(Uri uri)
        {
            bool isSecureScheme = uri.Scheme.Equals(secureScheme, StringComparison.OrdinalIgnoreCase);
            bool isSecurePort = uri.Port == securePort;

            return isSecureScheme && isSecurePort;
        }
    }
}