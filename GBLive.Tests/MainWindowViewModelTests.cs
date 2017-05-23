using System;
using System.Threading;
using System.Threading.Tasks;
using GBLive.WPF;
using NUnit.Framework;

namespace GBLive.Tests
{
    [TestFixture]
    public class MainWindowViewModelTests
    {
        private GBSettings dummySettings = new GBSettings();

        [Test]
        public void MainWindowViewModel_Ctor_ThrowArgNullWhenParamIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new MainWindowViewModel(dummySettings, null));
            Assert.Throws<ArgumentNullException>(() => new MainWindowViewModel(null, new TestWebRetriever()));
        }

        [Test]
        public void MainWindowViewModel_Instantiation_EventsIsEmpty()
        {
            var vm = new MainWindowViewModel(dummySettings, new TestWebRetriever());

            int expected = 0;
            int actual = vm.Events.Count;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public async Task MainWindowViewModel_UpdateAsync_EmptyStringShouldReturnNoEvents()
        {
            var vm = new MainWindowViewModel(dummySettings, new TestWebRetriever());

            await vm.UpdateAsync();

            int expected = 0;
            int actual = vm.Events.Count;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public async Task MainWindowViewModel_UpdateAsync_EmptyStringShouldBeNotLive()
        {
            var vm = new MainWindowViewModel(dummySettings, new TestWebRetriever());

            await vm.UpdateAsync();

            bool expected = false;
            bool actual = vm.IsLive;

            Assert.AreEqual(expected, actual);
        }
        
        [Test]
        public async Task MainWindowViewModel_UpdateAsync_IsNotLiveAndEvents()
        {
            string testData = Utilities.LoadTextFromFile("GBJson_NotLive_FiveEvents.json");
            
            var vm = new MainWindowViewModel(dummySettings, new TestWebRetriever(testData));

            await vm.UpdateAsync();
            
            bool expected = false;
            bool actual = vm.IsLive;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public async Task MainWindowViewModel_UpdateAsync_IsLiveAndEvents()
        {
            string testData = Utilities.LoadTextFromFile("GBJson_Live_FiveEvents.json");
            
            var vm = new MainWindowViewModel(dummySettings, new TestWebRetriever(testData));

            await vm.UpdateAsync();
            
            bool expected = true;
            bool actual = vm.IsLive;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public async Task MainWindowViewModel_UpdateAsync_FiveEvents()
        {
            string testData = Utilities.LoadTextFromFile("GBJson_Live_FiveEvents.json");
            
            var vm = new MainWindowViewModel(dummySettings, new TestWebRetriever(testData));

            await vm.UpdateAsync();

            int expected = 5;
            int actual = vm.Events.Count;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public async Task MainWindowViewModel_UpdateAsync_LiveShowNameSetCorrectly()
        {
            string testData = Utilities.LoadTextFromFile("GBJson_Live_FiveEvents.json");
            
            var vm = new MainWindowViewModel(dummySettings, new TestWebRetriever(testData));

            await vm.UpdateAsync();

            string expected = "GBE Playdate";
            string actual = vm.LiveShowName;
            
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public async Task MainWindowViewModel_UpdateAsync_EventShouldNotBeAddedWhenEqualAlreadyPresent()
        {
            string testData = Utilities.LoadTextFromFile("GBJson_Live_TwoEqualEvents.json");
            
            var vm = new MainWindowViewModel(dummySettings, new TestWebRetriever(testData));

            await vm.UpdateAsync();

            // two events in json, that are identical, only one should be added

            int expected = 1;
            int actual = vm.Events.Count;

            Assert.AreEqual(expected, actual);
        }
    }
}
