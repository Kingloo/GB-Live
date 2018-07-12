﻿using System;
using System.IO;
using GBLive.WPF.GiantBomb;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using NUnit.Framework;

namespace GBLive.Tests.GiantBomb
{
    [TestFixture]
    public class GiantBombServiceTests
    {
        [Test]
        public void Ctor_WhenSchemaNull_ThrowsArgNullExc()
        {
            Assert.Throws<ArgumentNullException>(() => new GiantBombService(schema: null));
        }

        [Test]
        public void Ctor_WhenClientNull_ThrowsArgNullExc()
        {
            Assert.Throws<ArgumentNullException>(() => new GiantBombService(client: null, schema: new JSchema()));
        }

        //[TestCase(HttpStatusCode.NotModified)]
        //[TestCase(HttpStatusCode.NotFound)]
        //[TestCase(HttpStatusCode.Unauthorized)]
        //[TestCase(HttpStatusCode.BadGateway)]
        //[TestCase(HttpStatusCode.BadRequest)]
        //public async Task UpdateAsync_StatusCodeNotOk_UpcomingResponseIsNotSuccessful(HttpStatusCode statusCode)
        //{
        //    using (var client = new TestHttpClient(statusCode, null))
        //    {
        //        var gbService = new GiantBombService(client, new JSchema());

        //        UpcomingResponse response = await gbService.UpdateAsync();

        //        bool expected = false;
        //        bool actual = response.IsSuccessful;

        //        Assert.AreEqual(expected, actual);

        //        Assert.IsNotNull(response.Events);
        //        Assert.Zero(response.Events.Count);
        //    }
        //}

        //[Test]
        //public async Task UpdateAsync_StatusCodeOkResponseStringEmpty_ReasonStringEmpty()
        //{
        //    using (var client = new TestHttpClient(HttpStatusCode.OK, new StringContent(string.Empty)))
        //    {
        //        var gbService = new GiantBombService(client, new JSchema());

        //        UpcomingResponse response = await gbService.UpdateAsync();

        //        Reason expected = Reason.StringEmpty;
        //        Reason actual = response.Reason;

        //        Assert.AreEqual(expected, actual);
        //        Assert.IsFalse(response.IsSuccessful);
        //    }
        //}

        //[TestCase("fish")]
        //[TestCase("sdvsdklvn")]
        //public async Task UpdateAsync_StatusCodeOkStringDoesNotParse_ReasonParseFailed(string text)
        //{
        //    var schema = LoadSchema();

        //    using (var client = new TestHttpClient(HttpStatusCode.OK, new StringContent(text)))
        //    {
        //        var gbService = new GiantBombService(client, schema);

        //        UpcomingResponse response = await gbService.UpdateAsync();

        //        Reason expected = Reason.ParseFailed;
        //        Reason actual = response.Reason;

        //        Assert.AreEqual(expected, actual);
        //        Assert.IsFalse(response.IsSuccessful);
        //    }
        //}

        // valid as json, but not valid GB Upcoming json
        //[TestCase("{\"firstName\":\"John\",\"lastName\":\"Smith\",\"address\":{\"streetAddress\":\"21 2nd Street\"},\"phoneNumbers\":[{\"type\":\"home\"},{\"type\":\"office\"}]}")]
        //public async Task UpdateAsync_StatusCodeOkJsonDoesNotValidate_ReasonValidateFailed(string text)
        //{
        //    var schema = LoadSchema();

        //    using (var client = new TestHttpClient(HttpStatusCode.OK, new StringContent(text)))
        //    {
        //        var gbService = new GiantBombService(client, schema);

        //        UpcomingResponse response = await gbService.UpdateAsync();

        //        Reason expected = Reason.ValidateFailed;
        //        Reason actual = response.Reason;

        //        Assert.AreEqual(expected, actual);
        //        Assert.IsFalse(response.IsSuccessful);
        //    }
        //}

        // valid GB Upcoming json, not live, 2 events
        //[TestCase(
        //    "{\"liveNow\":null,\"upcoming\":[{\"type\":\"Video\",\"title\":\"Quick Look\",\"image\":\"static.giantbomb.com/subaeria.jpg\",\"date\":\"May 20, 2018 06:00 AM\",\"premium\":false},{\"type\":\"Video\",\"title\":\"Quick Look #2\",\"image\":\"static.giantbomb.com/l002.jpg\",\"date\":\"May 21, 2018 06:00 AM\",\"premium\":false}]}",
        //    false,
        //    2)]
        //[TestCase(
        //    "{\"liveNow\":null,\"upcoming\":[{\"type\":\"Video\",\"title\":\"Quick Look\",\"image\":\"static.giantbomb.com/subaeria.jpg\",\"date\":\"May 20, 2018 06:00 AM\",\"premium\":false}]}",
        //    false,
        //    1)]
        //// valid GB Upcoming json, not live, zero events
        //[TestCase(
        //    "{\"liveNow\":null,\"upcoming\":null}",
        //    false,
        //    0)]
        //// valid GB Upcoming json, live, zero events
        //[TestCase(
        //    "{\"liveNow\":{\"title\":\"Live Show\",\"image\":\"static.giantbomb.com/image.jpg\"},\"upcoming\":null}",
        //    true,
        //    0)]
        //public async Task UpdateAsync_ValidJson_ParseSuccess(string json, bool isLive, int eventsCount)
        //{
        //    var schema = LoadSchema();

        //    using (var client = new TestHttpClient(HttpStatusCode.OK, new StringContent(json)))
        //    {
        //        var gbService = new GiantBombService(client, schema);

        //        UpcomingResponse response = await gbService.UpdateAsync();

        //        Reason expected = Reason.Success;
        //        Reason actual = response.Reason;

        //        Assert.AreEqual(expected, actual, "reasons differed");

        //        Assert.AreEqual(isLive, response.IsLive, "live status differed");
        //        Assert.AreEqual(eventsCount, response.Events.Count, "event count differed");
        //    }
        //}

        public JSchema LoadSchema()
        {
            JsonTextReader reader = new JsonTextReader(
                File.OpenText(
                    Path.Combine(
                        TestContext.CurrentContext.TestDirectory,
                        @"GiantBomb\schema.json")));

            return JSchema.Load(reader);
        }
    }
}
