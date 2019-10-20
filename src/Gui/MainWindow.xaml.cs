using System;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;

namespace GBLive.Gui
{
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel viewModel = null;

        public MainWindow(MainWindowViewModel vm)
        {
            if (vm is null) { throw new ArgumentNullException(nameof(vm)); }

            InitializeComponent();

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
            viewModel.GoToHome();
        }

        private void Label_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            viewModel.GoToChat();
        }
    }
}
