using System;
using System.IO;
using System.Threading.Tasks;
using GBLive.Common;
using GBLive.GiantBomb;
using GBLive.GiantBomb.Interfaces;
using NUnit.Framework;

namespace GBLive.Tests.GiantBomb
{
	[TestFixture]
	public class GiantBombContextTests
	{
		private readonly ILog nullLog = new NullLog();

		private TestServer? server;
		private const int port = 9876;
		private const string url = "127.0.0.1";

		private const string upcomingExamplesFolderName = "UpcomingJsonExamples";

		private const string notLiveNoShowsKey = "NotLiveNoShows";
		private const string notLiveOneShowKey = "NotLiveOneShow";
		private const string notLiveFourShowsKey = "NotLiveFourShows";

		[OneTimeSetUp]
		public async Task SetupAsync()
		{
			string notLiveNoShowsData = await LoadSampleAsync($"{notLiveNoShowsKey}.json").ConfigureAwait(false);
			string notLiveOneShowData = await LoadSampleAsync($"{notLiveOneShowKey}.json").ConfigureAwait(false);
			string notLiveFourShowsData = await LoadSampleAsync($"{notLiveFourShowsKey}.json").ConfigureAwait(false);

			server = new TestServer(port, url);

			server.Samples.Add(
				new Sample
				{
					Path = notLiveNoShowsKey,
					StatusCode = 200,
					ContentType = ContentTypes.JsonUTF8,
					Text = notLiveNoShowsData
				});

			server.Samples.Add(
				new Sample
				{
					Path = notLiveOneShowKey,
					StatusCode = 200,
					ContentType = ContentTypes.JsonUTF8,
					Text = notLiveOneShowData
				});

			server.Samples.Add(
				new Sample
				{
					Path = notLiveFourShowsKey,
					StatusCode = 200,
					ContentType = ContentTypes.JsonUTF8,
					Text = notLiveFourShowsData
				});

			await server.StartAsync().ConfigureAwait(false);
		}

		[TestCase(notLiveNoShowsKey, false, 0)]
		[TestCase(notLiveOneShowKey, false, 1)]
		[TestCase(notLiveFourShowsKey, false, 4)]
		public async Task IsLiveAndShowsCountAsync(string path, bool wereWeLive, int showsCount)
		{
			Settings settings = new Settings
			{
				Upcoming = new Uri($"http://{url}:{port}/{path}"),
				//UserAgent = "dummy user agent"
			};

			IGiantBombContext gbContext = new GiantBombContext(nullLog, settings);

			IResponse response = await gbContext.UpdateAsync().ConfigureAwait(false);

			Assert.IsTrue(response.Reason == Reason.Success, "reason should have been Success, actually was {0}", response.Reason);

			Assert.AreEqual(wereWeLive, response.LiveNow != null, "we should have been Live?:{0}, we were actually Live?:{1}", wereWeLive, response.LiveNow != null);
			Assert.AreEqual(showsCount, response.Shows.Count, "we should have had {0} shows, we actually had {}", showsCount, response.Shows.Count);
		}

		[OneTimeTearDown]
		public async Task TeardownAsync()
		{
			if (server != null)
			{
				await server.StopAsync().ConfigureAwait(false);

				server.Dispose();
			}
		}

		private static Task<string> LoadSampleAsync(string filename)
		{
			string buildOutputDir = Directory.GetCurrentDirectory();

			string fullPath = Path.Combine(buildOutputDir, upcomingExamplesFolderName, filename);

			return File.ReadAllTextAsync(fullPath);
		}
	}
}
