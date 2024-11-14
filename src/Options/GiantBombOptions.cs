using System;
using System.Diagnostics.CodeAnalysis;

namespace GBLive.Options
{
	public class GiantBombOptions
	{
		[DisallowNull]
		public Uri? HomePage { get; init; }
		[DisallowNull]
		public Uri? ChatPage { get; init; }
		[DisallowNull]
		public Uri? UpcomingJsonUri { get; init; }
		[DisallowNull]
		public Uri? FallbackImageUri { get; init; }
	}
}
