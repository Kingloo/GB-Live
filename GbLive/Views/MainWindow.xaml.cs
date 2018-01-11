using System;
using System.Windows;
using System.Windows.Input;
using GbLive.ViewModels;

namespace GbLive.Views
{
    public partial class MainWindow : Window, IDisposable
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
            => await viewModel.UpdateAsync();

        private async void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    Close();
                    break;
                case Key.F5:
                    await viewModel.UpdateAsync();
                    break;
                default:
                    break;
            }
        }

        private void EventList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
            => MainWindowViewModel.GoToHome();

        private void LiveLabel_MouseDoubleClick(object sender, MouseButtonEventArgs e)
            => MainWindowViewModel.GoToChat();
        
        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    (viewModel as IDisposable).Dispose();
                }
                
                disposedValue = true;
            }
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion        
    }
}
