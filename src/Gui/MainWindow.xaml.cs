using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using GBLive.Common;

namespace GBLive.Gui
{
    public partial class MainWindow : Window
    {
        private readonly ILog _logger;
        private readonly IMainWindowViewModel viewModel;

        public MainWindow(ILog logger, IMainWindowViewModel vm)
        {
            InitializeComponent();

            Loaded += Window_Loaded;
            KeyDown += Window_KeyDown;
            Closing += Window_Closing;

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
                    {
                        _logger.Message("pressed F5, update started", Severity.Debug);

                        await viewModel.UpdateAsync();

                        _logger.Message("update finished", Severity.Debug);

                        break;
                    }
                default:
                    break;
            }
        }

        private void ItemsControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Loaded -= Window_Loaded;
            KeyDown -= Window_KeyDown;
            Closing -= Window_Closing;
        }

        private void Image_Loaded(object sender, RoutedEventArgs e)
        {
            _logger.Message("image loaded", Severity.Debug);
        }

        private void Image_Unloaded(object sender, RoutedEventArgs e)
        {
            _logger.Message("image unloaded", Severity.Debug);
        }

        private void Image_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            _logger.Message("image failed", Severity.Debug);
        }
    }
}
