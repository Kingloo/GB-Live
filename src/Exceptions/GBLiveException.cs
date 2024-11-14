using System;

namespace GBLive.Exceptions
{
	internal sealed class GBLiveException : Exception
	{
		public GBLiveException()
		{ }

		public GBLiveException(string? message)
			: base(message)
		{ }

		public GBLiveException(string? message, Exception? innerException)
			: base(message, innerException)
		{ }
	}
}
