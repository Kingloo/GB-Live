using System.Diagnostics;
using System.Windows;

namespace GB_Live
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.F1)
            {
                foreach (GBUpcomingEvent each in vm.Events)
                {
                    //Debug.WriteLine(each.ToString());
                    Misc.LogMessage(each.ToString());
                }
            }
        }
    }
}
