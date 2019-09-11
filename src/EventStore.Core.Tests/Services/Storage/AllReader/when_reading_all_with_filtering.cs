using System;
using NUnit.Framework;
using EventStore.Core.Data;
using EventStore.Core.Messages;
using EventStore.Core.Util;
using static EventStore.Core.Messages.TcpClientMessageDto.Filter;

namespace EventStore.Core.Tests.Services.Storage.AllReader {
	public class when_reading_all_with_filtering : ReadIndexTestScenario {
		EventRecord _firstEvent;

		protected override void WriteTestScenario() {
			_firstEvent = WriteSingleEvent("ES1", 1, new string('.', 3000), eventId: Guid.NewGuid(),
				eventType: "event-type", retryOnFail: true);
			WriteSingleEvent("ES2", 1, new string('.', 3000), eventId: Guid.NewGuid(), eventType: "other-event-type",
				retryOnFail: true);
			WriteSingleEvent("ES3", 1, new string('.', 3000), eventId: Guid.NewGuid(), eventType: "event-type",
				retryOnFail: true);
			WriteSingleEvent("ES4", 1, new string('.', 3000), eventId: Guid.NewGuid(), eventType: "other-event-type",
				retryOnFail: true);
		}

		[Test]
		public void should_read_only_events_with_event_type_prefix() {
			var filter = new TcpClientMessageDto.Filter(
				FilterContext.EventType,
				FilterType.Prefix, "event-type");
			var eventFilter = new EventFilter(new[] {filter});
			
			var pos = new TFPos(_firstEvent.LogPosition, _firstEvent.LogPosition);
			var result = ReadIndex.ReadAllEventsForwardFiltered(pos, 10, 10, eventFilter, eventFilter);
			Assert.AreEqual(2, result.Records.Count);
		}
		
		[Test]
		public void should_read_only_events_with_event_type_regex() {
			var filter = new TcpClientMessageDto.Filter(
				FilterContext.EventType,
				FilterType.Regex, @"^.*other-event.*$");
			var eventFilter = new EventFilter(new[] {filter});
			
			var pos = new TFPos(_firstEvent.LogPosition, _firstEvent.LogPosition);
			var result = ReadIndex.ReadAllEventsForwardFiltered(pos, 10, 10, eventFilter, eventFilter);
			Assert.AreEqual(2, result.Records.Count);
		}
		
		[Test]
		public void should_read_only_events_with_stream_id_prefix() {
			var filter = new TcpClientMessageDto.Filter(
				FilterContext.StreamId,
				FilterType.Prefix, "ES2");
			var eventFilter = new EventFilter(new[] {filter});
			
			var pos = new TFPos(_firstEvent.LogPosition, _firstEvent.LogPosition);
			var result = ReadIndex.ReadAllEventsForwardFiltered(pos, 10, 10, eventFilter, eventFilter);
			Assert.AreEqual(1, result.Records.Count);
		}
		
		[Test]
		public void should_read_only_events_with_stream_id_regex() {
			var filter = new TcpClientMessageDto.Filter(
				FilterContext.StreamId,
				FilterType.Regex, @"^.*ES2.*$");
			var eventFilter = new EventFilter(new[] {filter});
			
			var pos = new TFPos(_firstEvent.LogPosition, _firstEvent.LogPosition);
			var result = ReadIndex.ReadAllEventsForwardFiltered(pos, 10, 10, eventFilter, eventFilter);
			Assert.AreEqual(1, result.Records.Count);
		}
	}
}
