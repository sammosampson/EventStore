using System;
using System.Linq;
using System.Text.RegularExpressions;
using EventStore.Core.Messages;
using EventStore.Core.TransactionLog.LogRecords;

namespace EventStore.Core.Util {
	public interface IEventFilter {
		bool IsEventAllowed(PrepareLogRecord prepareLogRecord);
	}
	
	public class EventFilter {
	
		public static IEventFilter Get(TcpClientMessageDto.Filter filter) {
			if (filter == null || filter.Data.Length == 0) {
				return new AlwaysAllowStrategy();
			}

			switch (filter.Context) {
				case TcpClientMessageDto.Filter.FilterContext.EventType
					when filter.Type == TcpClientMessageDto.Filter.FilterType.Prefix:
					return new EventTypePrefixStrategy(filter.Data);
				case TcpClientMessageDto.Filter.FilterContext.EventType
					when filter.Type == TcpClientMessageDto.Filter.FilterType.Regex:
					return new EventTypeRegexStrategy(filter.Data[0]);
				case TcpClientMessageDto.Filter.FilterContext.StreamId
					when filter.Type == TcpClientMessageDto.Filter.FilterType.Prefix:
					return new StreamIdPrefixStrategy(filter.Data);
				case TcpClientMessageDto.Filter.FilterContext.StreamId
					when filter.Type == TcpClientMessageDto.Filter.FilterType.Regex:
					return new StreamIdRegexStrategy(filter.Data[0]);
			}
			
			throw new Exception(); // Invalid filter
		}

		private class AlwaysAllowStrategy : IEventFilter {
			public bool IsEventAllowed(PrepareLogRecord prepareLogRecord) {
				return true;
			}
		}

		private class StreamIdPrefixStrategy : IEventFilter {
			private readonly string[] _expectedPrefixes;

			public StreamIdPrefixStrategy(string[] expectedPrefixes) =>
				_expectedPrefixes = expectedPrefixes;

			public bool IsEventAllowed(PrepareLogRecord prepareLogRecord) =>
				_expectedPrefixes.Any(expectedPrefix => prepareLogRecord.EventStreamId.StartsWith(expectedPrefix));
		}

		private class EventTypePrefixStrategy : IEventFilter {
			private readonly string[] _expectedPrefixes;

			public EventTypePrefixStrategy(string[] expectedPrefixes) =>
				_expectedPrefixes = expectedPrefixes;

			public bool IsEventAllowed(PrepareLogRecord prepareLogRecord) =>
				_expectedPrefixes.Any(expectedPrefix => prepareLogRecord.EventType.StartsWith(expectedPrefix));
		}

		private class EventTypeRegexStrategy : IEventFilter {
			private readonly Regex _expectedRegex;

			public EventTypeRegexStrategy(string expectedRegex) =>
				_expectedRegex = new Regex(expectedRegex, RegexOptions.Compiled);

			public bool IsEventAllowed(PrepareLogRecord prepareLogRecord) =>
				_expectedRegex.Match(prepareLogRecord.EventType).Success;
		}

		private class StreamIdRegexStrategy : IEventFilter {
			private readonly Regex _expectedRegex;

			public StreamIdRegexStrategy(string expectedRegex) =>
				_expectedRegex = new Regex(expectedRegex, RegexOptions.Compiled);

			public bool IsEventAllowed(PrepareLogRecord prepareLogRecord) =>
				_expectedRegex.Match(prepareLogRecord.EventStreamId).Success;
		}
	}
}
