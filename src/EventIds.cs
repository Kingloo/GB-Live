using Microsoft.Extensions.Logging;

namespace GBLive.EventIds
{
	internal static class App
	{
		internal const int StartupStartedId = 101;
		internal const int StartupFinishedId = 102;
		internal const int AppExitZeroId = 103;
		internal const int AppExitNotZeroId = 104;
		internal const int DispatcherUnhandledExceptionId = 105;

		internal static readonly EventId StartupStarted = new EventId(StartupStartedId, nameof(StartupStarted));
		internal static readonly EventId StartupFinished = new EventId(StartupFinishedId, nameof(StartupFinished));
		internal static readonly EventId AppExitZero = new EventId(AppExitZeroId, nameof(AppExitZero));
		internal static readonly EventId AppExitNotZero = new EventId(AppExitNotZeroId, nameof(AppExitNotZero));
		internal static readonly EventId DispatcherUnhandledException = new EventId(DispatcherUnhandledExceptionId, nameof(DispatcherUnhandledException));
	}
}