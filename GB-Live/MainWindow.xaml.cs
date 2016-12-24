using System;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;

namespace GB_Live
{
    public partial class MainWindow : Window
    {
        private IntPtr hWnd = IntPtr.Zero;

        public MainWindow()
        {
            InitializeComponent();

            KeyUp += MainWindow_KeyUp;
            SourceInitialized += MainWindow_SourceInitialized;
            LocationChanged += (s, e) => CalculateMaxHeight();
            Loaded += async (s, e) => await vm.UpdateAsync();
        }

        private void MainWindow_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    Exit();
                    break;
                case Key.F1:
                    WriteEventsToLog();
                    break;
                default:
                    break;
            }
        }

        private void MainWindow_SourceInitialized(object sender, EventArgs e)
        {
            hWnd = new WindowInteropHelper(this).EnsureHandle();
        }
        
        private void CalculateMaxHeight()
        {
            Screen currentMonitor = Screen.FromHandle(hWnd);

            MaxHeight = currentMonitor.WorkingArea.Bottom - 200;
        }
        
        private void WriteEventsToLog()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(Environment.NewLine);

            foreach (GBUpcomingEvent each in vm.Events)
            {
                sb.Append(each.ToString());
            }

            sb.Append(Environment.NewLine);

            Utils.LogMessage(sb.ToString());
        }
        
        private void Exit()
        {
            /*
             *  this will shutdown the programme
             *  because App.xaml->ShutdownMode
             *  is set to OnMainWindowClose
             */

            Close();
        }
    }
}



//private void DEBUG_Add_Test_Event()
//{
//    GBUpcomingEvent test = new GBUpcomingEvent
//        (Environment.TickCount.ToString(CultureInfo.InvariantCulture),
//        DateTime.Now + TimeSpan.FromSeconds(3),
//        Environment.TickCount % 2 == 0,
//        "test event",
//        new Uri("http://www.giantbomb.com/bundles/phoenixsite/images/core/loose/favicon-gb.ico"));

//    vm.DEBUG_add(test);
//}

//private void DEBUG_Toggle_Live()
//{
//    vm.DEBUG_set_live();
//}