using System;
using System.Threading;
using GBLive.WPF;
using NUnit.Framework;

namespace GBLive.Tests
{
    [TestFixture]
    public class MainWindowTests
    {
        [Test, Apartment(ApartmentState.STA)]
        public static void MainWindow_Ctor_ThrowArgNullWhenParamIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new MainWindow(null));
        }

        [Test, Apartment(ApartmentState.STA)]
        public void MainWindow_Ctor_IsDataContextSetToParam()
        {
            var vm = new MainWindowViewModel(new GBSettings(), new TestWebRetriever());

            var win = new MainWindow(vm);

            bool expected = true;
            bool actual = win.DataContext.Equals(vm);

            Assert.AreEqual(expected, actual);
        }
    }
}
