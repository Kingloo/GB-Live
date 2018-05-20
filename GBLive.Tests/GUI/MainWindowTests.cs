using System;
using System.Threading;
using GBLive.WPF.GUI;
using NUnit.Framework;

namespace GBLive.Tests.GUI
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class MainWindowTests
    {
        [Test]
        public void Ctor_ThrowsArgNull_WhenParamNull()
        {
            Assert.Throws<ArgumentNullException>(() => new MainWindow(null));
        }

        [Test]
        public void Ctor_DataContext_NotNull()
        {
            var window = new MainWindow(new MainWindowViewModel());

            Assert.IsNotNull(window.DataContext, "DataContext was null");
        }

        [Test]
        public void Ctor_DataContext_IsMainWindowViewModel()
        {
            var window = new MainWindow(new MainWindowViewModel());

            Assert.IsInstanceOf<MainWindowViewModel>(
                window.DataContext,
                $"DataContext should be {typeof(MainWindowViewModel)}, but was {window.DataContext.GetType()}");
        }

        [Test]
        public void CtorParam_SetToDataContext()
        {
            var viewModel = new MainWindowViewModel();
            var window = new MainWindow(viewModel);

            Assert.AreEqual(viewModel, (MainWindowViewModel)window.DataContext);
        }
    }
}
