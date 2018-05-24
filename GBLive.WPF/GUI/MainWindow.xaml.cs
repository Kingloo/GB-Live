using System;
using System.Windows;
using System.Windows.Input;

namespace GBLive.WPF.GUI
{
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel viewModel = null;

        public MainWindow(MainWindowViewModel viewModel)
        {
            this.viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

            InitializeComponent();

            DataContext = this.viewModel;
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
                    await viewModel.LogEverythingAsync();
                    break;
                case Key.F5:
                    await viewModel.UpdateAsync();
                    break;
                default:
                    break;
            }
        }
    }
}
