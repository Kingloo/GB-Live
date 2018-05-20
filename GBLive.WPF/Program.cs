using System;
using GBLive.WPF.GUI;

namespace GBLive.WPF
{
    public static class Program
    {
        [STAThread]
        public static int Main()
        {
            using (var viewModel = new MainWindowViewModel(autoStartTimer: true))
            {
                var window = new MainWindow(viewModel);

                window.ShowDialog();
            }

            return 0;
        }
    }
}
