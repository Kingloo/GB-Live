using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using Microsoft.Extensions.Logging;

namespace GBLive.Gui
{
    public partial class MainWindow : Window
    {
        private readonly ILogger<MainWindow> _logger;
        private readonly IMainWindowViewModel viewModel;

        public MainWindow(ILogger<MainWindow> logger, IMainWindowViewModel vm)
        {
            InitializeComponent();

            _logger = logger;

            Language = XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag);

            viewModel = vm;

            DataContext = viewModel;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await viewModel.UpdateAsync();

            viewModel.StartTimer();
        }

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

        private void ItemsControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            _logger.LogInformation("mouse button changed: {0}", e.ChangedButton);

            if (e.ChangedButton == MouseButton.Left)
            {
                viewModel.OpenHomePage();
            }
        }

        private void Label_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                viewModel.OpenChatPage();
            }
        }
    }
}
