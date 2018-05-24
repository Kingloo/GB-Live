using System;
using GBLive.WPF.GiantBomb;
using GBLive.WPF.GUI;
using NUnit.Framework;

namespace GBLive.Tests.GUI
{
    [TestFixture]
    public class MainWindowViewModelTests
    {
        [Test]
        public void IsLive_DefaultFalse()
        {
            using (var vm = new MainWindowViewModel())
            {
                Assert.IsFalse(vm.IsLive);
            }
        }

        [Test]
        public void LiveShowName_DefaultMatchesFromSettings()
        {
            using (var vm = new MainWindowViewModel())
            {
                string expected = Settings.NameOfNoLiveShow;
                string actual = vm.LiveShowName;

                Assert.AreEqual(expected, actual);
            }
        }

        [Test]
        public void Events_NotNullAndEmpty()
        {
            using (var vm = new MainWindowViewModel())
            {
                Assert.IsNotNull(vm.Events);

                Assert.Zero(
                    vm.Events.Count,
                    $"Events actually had {vm.Events.Count} events");
            }
        }

        [Test]
        public void UpdateInterval_TimerNull_Zero()
        {
            using (var vm = new MainWindowViewModel())
            {
                Assert.AreEqual(
                    TimeSpan.Zero,
                    vm.UpdateInterval,
                    $"timer running: {vm.IsUpdateTimerRunning}");
            }
        }

        [Test]
        public void UpdateInterval_TimerRunning_SameAsFromSettings()
        {
            using (var vm = new MainWindowViewModel(autoStartTimer: true))
            {
                Assert.AreEqual(Settings.UpdateInterval, vm.UpdateInterval);
            }
        }

        [Test]
        public void Ctor_Parameterless_TimerDoesNotStart()
        {
            using (var vm = new MainWindowViewModel())
            {
                Assert.IsFalse(vm.IsUpdateTimerRunning);
            }
        }

        [Test]
        public void StartTimer_StartsTimer()
        {
            using (var vm = new MainWindowViewModel())
            {
                vm.StartTimer();

                Assert.IsTrue(vm.IsUpdateTimerRunning);
            }
        }

        [Test]
        public void StopTimer_StopsTimer()
        {
            using (var vm = new MainWindowViewModel(autoStartTimer: true))
            {
                vm.StopTimer();

                Assert.IsFalse(vm.IsUpdateTimerRunning);
            }
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Ctor_AutoStart(bool autoStart)
        {
            using (var vm = new MainWindowViewModel(autoStartTimer: autoStart))
            {
                Assert.AreEqual(autoStart, vm.IsUpdateTimerRunning);
            }
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Ctor_ServiceIsNull_ThrowsArgNullExc(bool shouldAutoStart)
        {
            Assert.Throws<ArgumentNullException>(() => new MainWindowViewModel(service: null));
            Assert.Throws<ArgumentNullException>(() => new MainWindowViewModel(service: null, autoStartTimer: shouldAutoStart));
        }
    }
}
