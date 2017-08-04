using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using GBLive.Desktop.Common;

namespace GBLive.Desktop.Views
{
    public partial class MainWindow : Window
    {
        #region Fields
        private const string appName = "GB Live";
        private IntPtr hWnd = IntPtr.Zero;
        private readonly IViewModel _viewModel = null;
        #endregion

        public MainWindow(IViewModel viewModel)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

            InitializeComponent();

            DataContext = _viewModel;

            Loaded += MainWindow_Loaded;
            KeyDown += MainWindow_KeyDown;
            SourceInitialized += MainWindow_SourceInitialized;
            LocationChanged += MainWindow_LocationChanged;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
            => await _viewModel.UpdateAsync();
        
        private async void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    Close();
                    break;
                case Key.F1:
                    await LogEventsAsync();
                    break;
                case Key.F5:
                    await ManualUpdateAsync();
                    break;
                default:
                    break;
            }
        }
        
        private async Task LogEventsAsync()
        {
            StringBuilder sb = new StringBuilder();

            foreach (var gb in _viewModel.Events)
            {
                sb.AppendLine(gb.ToString());
            }

            await Log.LogMessageAsync(sb.ToString()).ConfigureAwait(false);
        }

        private async Task ManualUpdateAsync()
        {
            var task = Task.Run(() => _viewModel.UpdateAsync());

            Opacity = 0.5d;
            Title = string.Format(CultureInfo.CurrentCulture, "{0}: updating...", appName);
            
            await task;

            Opacity = 1.0d;
            Title = appName;
        }
        
        private void MainWindow_SourceInitialized(object sender, EventArgs e)
        {
            hWnd = new WindowInteropHelper(this).EnsureHandle();

            CalculateMaxHeight();
        }

        private void MainWindow_LocationChanged(object sender, EventArgs e)
            => CalculateMaxHeight();

        private void CalculateMaxHeight()
        {
            var currentMonitor = System.Windows.Forms.Screen.FromHandle(hWnd);

            MaxHeight = currentMonitor.WorkingArea.Bottom - 200;
        }

        private void LiveLabel_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_viewModel.IsLive)
            {
                _viewModel.GoToChatPage();
            }
            else
            {
                _viewModel.GoToHomepage();
            }
        }

        private void EventList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
            => _viewModel.GoToHomepage();

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine(GetType().FullName);
            sb.AppendLine(_viewModel.ToString());

            return sb.ToString();
        }
    }
}