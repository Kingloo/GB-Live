using System;
using System.Diagnostics;
using System.IO;

namespace GBLive.Common
{
	internal static class SystemLaunch
	{
		public static bool Path(string path)
		{
			ArgumentException.ThrowIfNullOrWhiteSpace(path);

			return File.Exists(path) && Launch(path);
		}

		public static bool Uri(Uri uri)
		{
			ArgumentNullException.ThrowIfNull(uri);

			return uri.IsAbsoluteUri && Launch(uri.AbsoluteUri);
		}

		public static bool Launch(string launchString)
		{
			if (String.IsNullOrWhiteSpace(launchString))
			{
				throw new ArgumentNullException(nameof(launchString));
			}

			ProcessStartInfo pInfo = new ProcessStartInfo(launchString)
			{
				UseShellExecute = true
			};

			return LaunchInternal(pInfo);
		}

		public static bool Launch(ProcessStartInfo pInfo)
		{
			ArgumentNullException.ThrowIfNull(pInfo);

			return LaunchInternal(pInfo);
		}

		private static bool LaunchInternal(ProcessStartInfo pInfo)
		{
			using Process p = new Process
			{
				StartInfo = pInfo
			};

			return p.Start();
		}
	}
}
