using System;
using System.Globalization;
using GBLive.Common;
using GBLive.Gui;

namespace GBLive
{
	public static class Program
	{
		[STAThread]
		public static int Main()
		{
			App app = new App();

			int exitCode = app.Run();

			if (exitCode != 0)
			{
				string message = string.Format(CultureInfo.CurrentCulture, "exited with code: {0}", exitCode);

				LogStatic.Message(message);
			}

			return exitCode;
		}
	}
}
