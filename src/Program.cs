using System;
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

			return exitCode;
		}
	}
}
