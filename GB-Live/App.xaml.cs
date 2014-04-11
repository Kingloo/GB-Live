using System;
using System.IO;
using System.Windows;

namespace GB_Live
{
    public partial class App : Application
    {
        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            string logFilePath = string.Format("C:\\Users\\{0}\\Documents\\logfile.txt", Environment.UserName);

            using (StreamWriter sw = new StreamWriter(logFilePath))
            {
                sw.WriteLine(string.Format("{0}{1}: {2}: {3}: {4}{0}", Environment.NewLine, DateTime.Now, Application.Current.ToString(), e.Exception.ToString(), e.Exception.Message.ToString()));
            }

            e.Handled = false;
        }
    }
}
