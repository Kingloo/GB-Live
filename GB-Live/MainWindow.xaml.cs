using System;
using System.Globalization;
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

            //KeyUp += MainWindow_KeyUp;
        }

        //private void MainWindow_KeyUp(object sender, KeyEventArgs e)
        //{
        //    if (e.Key == Key.F1)
        //    {
        //        GBUpcomingEvent test = new GBUpcomingEvent
        //            (Environment.TickCount.ToString(CultureInfo.InvariantCulture),
        //            DateTime.Now,
        //            Environment.TickCount % 2 == 1,
        //            GBEventType.Unknown,
        //            new Uri("http://www.giantbomb.com/bundles/phoenixsite/images/core/loose/favicon-gb.ico"));

        //        vm.Events.Add(test);
        //    }

        //    if (e.Key == Key.F2)
        //    {
        //        vm.DEBUG_set_live();
        //    }
        //}
    }
}