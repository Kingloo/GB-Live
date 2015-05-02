using System;

namespace GB_Live
{
    public class Program
    {
        [STAThread]
        public static int Main(string[] args)
        {
            App app = new App();
            app.InitializeComponent();

            int exitCode = app.Run();

            if (exitCode != 0)
            {
                string errorMessage = string.Format("Exited with code {1}", exitCode);

                Utils.LogMessage(errorMessage);
            }

            return exitCode;
        }
    }
}
