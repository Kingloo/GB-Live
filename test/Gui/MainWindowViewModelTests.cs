using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GBLive.Common;
using GBLive.GiantBomb;
using GBLive.GiantBomb.Interfaces;
using GBLive.Gui;
using NUnit.Framework;

namespace GBLive.Tests.Gui
{
	[TestFixture]
	public class MainWindowViewModelTests
	{
		private readonly ILog nullLog = new NullLog();
		private readonly ISettings settings = new Settings();

		private readonly IReadOnlyCollection<IShow> shows = new List<IShow>
		{
			new Show
			{
				Title = "show1",
				Time = DateTimeOffset.Now.AddDays(1d)
			},
			new Show
			{
				Title = "show2",
				Time = DateTimeOffset.Now.AddDays(2d)
			},
			new Show
			{
				Title = "show3",
				Time = DateTimeOffset.Now.AddDays(3d)
			}
		}
		.AsReadOnly();

		[Test]
		public async Task EmptyResponse_NoShows()
		{
			IResponse response = new Response
			{
				Reason = Reason.Success
			};

			var fakeContext = new FakeGiantBombContext
			{
				Response = response
			};

			using var viewModel = new MainWindowViewModel(nullLog, fakeContext, settings);

			await viewModel.UpdateAsync().ConfigureAwait(false);

			Assert.Zero(viewModel.Shows.Count);
		}

		[Test]
		public async Task EmptyResponse_NotLive()
		{
			IResponse response = new Response
			{
				Reason = Reason.Success
			};

			var fakeContext = new FakeGiantBombContext
			{
				Response = response
			};

			using var viewModel = new MainWindowViewModel(nullLog, fakeContext, settings);

			await viewModel.UpdateAsync().ConfigureAwait(false);

			Assert.IsFalse(viewModel.IsLive);
		}

		[Test]
		public async Task FullResponse_AllShows()
		{
			IResponse response = new Response
			{
				Reason = Reason.Success,
				Shows = shows
			};

			var fakeContext = new FakeGiantBombContext
			{
				Response = response
			};

			using var viewModel = new MainWindowViewModel(nullLog, fakeContext, settings);

			await viewModel.UpdateAsync().ConfigureAwait(false);

			Assert.AreEqual(shows.Count, viewModel.Shows.Count);
		}

		[TestCase(true)]
		[TestCase(false)]
		public async Task FullResponse_IsLive(bool isLive)
		{
			IResponse response = new Response
			{
				Reason = Reason.Success
			};

			response.LiveNow = isLive ? new LiveNowData() : null;

			var fakeContext = new FakeGiantBombContext
			{
				Response = response
			};

			using var viewModel = new MainWindowViewModel(nullLog, fakeContext, settings);

			await viewModel.UpdateAsync().ConfigureAwait(false);

			bool expected = isLive;
			bool actual = viewModel.IsLive;

			Assert.AreEqual(expected, actual);
		}
	}
}
