using GBLive.GiantBomb;
using GBLive.GiantBomb.Interfaces;
using NUnit.Framework;

namespace GBLive.Tests.GiantBomb
{
	[TestFixture]
	public class ResponseTests
	{
		[Test]
		public void ShowsBeginsEmpty()
		{
			IResponse response = new Response();

			Assert.Zero(response.Shows.Count);
		}
	}
}
