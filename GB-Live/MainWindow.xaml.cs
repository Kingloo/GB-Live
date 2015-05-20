using System;
using System.Text;
using System.Windows;

namespace GB_Live
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.F1)
            {
                StringBuilder sb = new StringBuilder();

                if (vm.Events.Count > 0)
                {
                    foreach (GBUpcomingEvent each in vm.Events)
                    {
                        sb.AppendLine(each.ToString());
                    }
                }
                else
                {
                    sb.AppendLine("No events");
                }

                await Utils.LogMessageAsync(sb.ToString()).ConfigureAwait(false);
            }

            //if (e.Key == System.Windows.Input.Key.F2)
            //{
            //    test = new GBUpcomingEvent("test", DateTime.Now + new TimeSpan(0, 0, 4), true, GBEventType.Unknown);

            //    vm.Events.Add(test);
            //}

            //if (e.Key == System.Windows.Input.Key.F3)
            //{
            //    vm.Events.Remove(test);
            //}

            //if (e.Key == System.Windows.Input.Key.F4)
            //{
            //    NotificationService.Send("test2", "desc");
            //}
        }

        //private GBUpcomingEvent test;
    }
}
