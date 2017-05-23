using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace GBLive.WPF
{
    public partial class MainWindow : Window
    {
        #region Fields
        private const string appName = "GB Live";
        private IntPtr hWnd = IntPtr.Zero;
        private readonly IViewModel viewModel = null;
        #endregion

        public MainWindow(IViewModel viewmodel)
        {
            viewModel = viewmodel ?? throw new ArgumentNullException(nameof(viewmodel));

            InitializeComponent();

            DataContext = viewModel;

            Loaded += MainWindow_Loaded;
            KeyUp += MainWindow_KeyUp;
            SourceInitialized += MainWindow_SourceInitialized;
            LocationChanged += MainWindow_LocationChanged;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await viewModel.UpdateAsync();
        }
        
        private async void MainWindow_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    Close();
                    break;
                case Key.F5:
                    await ManualUpdateAsync();
                    break;
                default:
                    break;
            }
        }

        private async Task ManualUpdateAsync()
        {
            Opacity = 0.5d;
            Title = string.Format(CultureInfo.CurrentCulture, "{0}: updating...", appName);

            await viewModel.UpdateAsync();

            Opacity = 1.0d;
            Title = appName;
        }
        
        private void MainWindow_SourceInitialized(object sender, EventArgs e)
        {
            hWnd = new WindowInteropHelper(this).EnsureHandle();

            CalculateMaxHeight();
        }

        private void MainWindow_LocationChanged(object sender, EventArgs e)
        {
            CalculateMaxHeight();
        }

        private void CalculateMaxHeight()
        {
            var currentMonitor = System.Windows.Forms.Screen.FromHandle(hWnd);

            MaxHeight = currentMonitor.WorkingArea.Bottom - 200;
        }

        private void Label_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (viewModel.IsLive)
            {
                viewModel.GoToChatPage();
            }
            else
            {
                viewModel.GoToHomePage();
            }
        }

        private void ItemsControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            viewModel.GoToHomePage();
        }
    }
}