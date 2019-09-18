using System;
using System.Linq;
using System.Net;
using EventStore.Core.Tests.Http.Users.users;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using HttpStatusCode = System.Net.HttpStatusCode;
using EventStore.Transport.Http;

namespace EventStore.Core.Tests.Http.Streams {
	public class filtered {
		public abstract class SpecificationWithLongFeed : with_admin_user {
			protected int NumberOfEvents;

			protected override void Given() {
				NumberOfEvents = 25;
				for (var i = 0; i < NumberOfEvents; i++) {
					PostEvent(i, "filtered-event-type");
					PostEvent(i, "event-type");
				}
			}

			protected string PostEvent(int i, string eventType) {
				var response = MakeArrayEventsPost(
					TestStream,
					new[] {new {EventId = Guid.NewGuid(), EventType = eventType, Data = new {Number = i}}});
				Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
				return response.Headers[HttpResponseHeader.Location];
			}

			protected string GetLink(JObject feed, string relation) {
				var rel = (from JObject link in feed["links"]
					from JProperty attr in link
					where attr.Name == "relation" && (string)attr.Value == relation
					select link).SingleOrDefault();
				return (rel == null) ? null : (string)rel["uri"];
			}

			protected string AllFilteredStream => "/streams/%24all/filtered";
		}

		[TestFixture, Category("LongRunning")]
		public class when_retrieving_with_invalid_context : SpecificationWithLongFeed {
			protected override void When() =>
				GetJson<JObject>(
					AllFilteredStream + "?context=foo",
					ContentType.AtomJson,
					DefaultData.AdminNetworkCredentials);


			[Test]
			public void returns_bad_request_status_code() =>
				Assert.AreEqual(HttpStatusCode.BadRequest, _lastResponse.StatusCode);

			[Test]
			public void returns_status_description() =>
				Assert.AreEqual(
					"Invalid context please provide one of the following: StreamId, EventType",
					_lastResponse.StatusDescription);
		}

		[TestFixture, Category("LongRunning")]
		public class when_retrieving_with_invalid_type : SpecificationWithLongFeed {
			protected override void When() =>
				GetJson<JObject>(AllFilteredStream + "?context=streamid&type=foo",
					ContentType.AtomJson,
					DefaultData.AdminNetworkCredentials);

			[Test]
			public void returns_bad_request_status_code() =>
				Assert.AreEqual(HttpStatusCode.BadRequest, _lastResponse.StatusCode);

			[Test]
			public void returns_status_description() =>
				Assert.AreEqual(
					"Invalid type please provide one of the following: Regex, Prefix",
					_lastResponse.StatusDescription);
		}

		[TestFixture, Category("LongRunning")]
		public class when_retrieving_with_invalid_data : SpecificationWithLongFeed {
			protected override void When() =>
				GetJson<JObject>(AllFilteredStream + "?context=streamid&type=foo",
					ContentType.AtomJson,
					DefaultData.AdminNetworkCredentials);

			[Test]
			public void returns_bad_request_status_code() =>
				Assert.AreEqual(HttpStatusCode.BadRequest, _lastResponse.StatusCode);

			[Test]
			public void returns_status_description() =>
				Assert.AreEqual(
					"Please provide a comma delimited list of data with at least one item",
					_lastResponse.StatusDescription);
		}

		[TestFixture, Category("LongRunning")]
		public class when_retrieving_feed_head : SpecificationWithLongFeed {
			private JObject _feed;

			protected override void When() {
				_feed = GetJson<JObject>(AllFilteredStream+ "?context=eventtype&type=prefix&data=event-", ContentType.AtomJson, DefaultData.AdminNetworkCredentials);
			}

			[Test]
			public void returns_ok_status_code() {
				Assert.AreEqual(HttpStatusCode.OK, _lastResponse.StatusCode);
			}

			[Test]
			public void contains_a_link_rel_previous() {
				var rel = GetLink(_feed, "previous");
				Assert.IsNotEmpty(rel);
				Assert.AreEqual(
					MakeUrl(AllFilteredStream + "/0000000000001EAC0000000000001EAC/forward/20"),
					new Uri(rel));
			}

			[Test]
			public void contains_a_link_rel_next() {
				var rel = GetLink(_feed, "next");
				Assert.IsNotEmpty(rel);
				Assert.AreEqual(
					MakeUrl(AllFilteredStream + "/00000000000014A200000000000014A2/backward/20"),
					new Uri(rel));
			}

			[Test]
			public void contains_a_link_rel_self() {
				var rel = GetLink(_feed, "self");
				Assert.IsNotEmpty(rel);
				Assert.AreEqual(
					MakeUrl(AllFilteredStream),
					new Uri(rel));
			}

			[Test]
			public void contains_a_link_rel_first() {
				var rel = GetLink(_feed, "first");
				Assert.IsNotEmpty(rel);
				Assert.AreEqual(
					MakeUrl(AllFilteredStream + "/head/backward/20"),
					new Uri(rel));
			}

			[Test]
			public void contains_a_link_rel_last() {
				var rel = GetLink(_feed, "last");
				Assert.IsNotEmpty(rel);
				Assert.AreEqual(
					MakeUrl(AllFilteredStream + "/00000000000000000000000000000000/forward/20"),
					new Uri(rel));
			}

			[Test]
			public void contains_a_link_rel_metadata() {
				var rel = GetLink(_feed, "metadata");
				Assert.IsNotEmpty(rel);
				Assert.AreEqual(
					MakeUrl(AllFilteredStream + "/metadata"),
					new Uri(rel));
			}
		}
	}
}
