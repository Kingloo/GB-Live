using System;
using NUnit.Framework;
using static GBLive.WPF.GiantBomb.Settings;

namespace GBLive.Tests.GiantBomb
{
    [TestFixture]
    public class SettingsTests
    {
        [Test]
        public void NoSettingIsBlank()
        {
            Assert.IsNotEmpty(IsLiveMessage);
            Assert.IsNotEmpty(IsNotLiveMessage);
            Assert.IsNotEmpty(NameOfUntitledLiveShow);
            Assert.IsNotEmpty(NameOfNoLiveShow);
            Assert.IsNotEmpty(UserAgent);

            Assert.IsNotNull(Home);
            Assert.IsNotNull(Chat);
            Assert.IsNotNull(Upcoming);
            Assert.IsNotNull(FallbackImage);
        }

        [Test]
        public void AllUris_HaveGiantBombDomain()
        {
            string domain = "giantbomb.com";

            var sc = StringComparison.OrdinalIgnoreCase;

            bool doesHome = Home.DnsSafeHost.EndsWith(domain, sc);
            bool doesChat = Chat.DnsSafeHost.EndsWith(domain, sc);
            bool doesUpcoming = Upcoming.DnsSafeHost.EndsWith(domain, sc);
            bool doesFallbackImage = FallbackImage.DnsSafeHost.EndsWith(domain, sc);

            Assert.IsTrue(doesHome && doesChat && doesUpcoming && doesFallbackImage);
        }

        [Test]
        public void AllUris_AreSsl()
        {
            string scheme = "https";
            int port = 443;

            var sc = StringComparison.OrdinalIgnoreCase;

            bool isHome = Home.Scheme.Equals(scheme, sc) && Home.Port == port;
            bool isChat = Chat.Scheme.Equals(scheme, sc) && Chat.Port == port;
            bool isUpcoming = Upcoming.Scheme.Equals(scheme, sc) && Upcoming.Port == port;
            bool isFallbackImage = FallbackImage.Scheme.Equals(scheme, sc) && FallbackImage.Port == port;

            Assert.IsTrue(isHome && isChat && isUpcoming && isFallbackImage);
        }

        [Test]
        public void AllUris_AreAbsolute()
        {
            Assert.IsTrue(Home.IsAbsoluteUri
                && Chat.IsAbsoluteUri
                && Upcoming.IsAbsoluteUri
                && FallbackImage.IsAbsoluteUri);
        }

        [Test]
        public void UpdateInterval_WithinAcceptableRange()
        {
            var lowest = TimeSpan.FromSeconds(45d);
            var highest = TimeSpan.FromMinutes(5d);

            bool actual = (UpdateInterval >= lowest) && (UpdateInterval <= highest);

            Assert.IsTrue(actual);
        }
    }
}
