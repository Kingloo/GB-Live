using System.Windows;
using System.Windows.Input;
using GbLive.Common;

namespace GbLive.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await viewModel.UpdateAsync();
        }

        private async void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    Close();
                    break;
                case Key.F1:
                    Log.LogMessage(viewModel.ToString());
                    break;
                case Key.F5:
                    await viewModel.UpdateAsync();
                    break;
                default:
                    break;
            }
        }

        private void EventList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            viewModel.GoToHome();
        }

        private void LiveLabel_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            viewModel.GoToChat();
        }
    }
}
