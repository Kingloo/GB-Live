using System;
using System.Windows;

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
    }
}
