using System;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace GB_Live
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            MaxHeight = SystemParameters.WorkArea.Bottom - 200;

            Loaded += async (sender, e) => await vm.UpdateAsync();

            KeyUp += MainWindow_KeyUp;
        }

        private void MainWindow_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    Exit();
                    break;
                case Key.F1:
                    WriteEventsToLog();
                    break;
                case Key.F2:
                    DEBUG_Add_Test_Event();
                    break;
                case Key.F3:
                    DEBUG_Toggle_Live();
                    break;
            }
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

        private void DEBUG_Add_Test_Event()
        {
            GBUpcomingEvent test = new GBUpcomingEvent
                (Environment.TickCount.ToString(CultureInfo.InvariantCulture),
                DateTime.Now + TimeSpan.FromSeconds(3),
                Environment.TickCount % 2 == 0,
                GBEventType.Unknown,
                new Uri("http://www.giantbomb.com/bundles/phoenixsite/images/core/loose/favicon-gb.ico"));

#if DEBUG
            vm.DEBUG_add(test);
#endif
        }

        private void DEBUG_Toggle_Live()
        {
#if DEBUG
            vm.DEBUG_set_live();
#endif
        }
    }
}