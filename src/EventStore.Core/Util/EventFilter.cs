using System.Collections.Generic;
using System.Text.RegularExpressions;
using EventStore.Core.Messages;
using EventStore.Core.TransactionLog.LogRecords;

namespace EventStore.Core.Util {
	public class EventFilter {
		private interface IFilterStrategy {
			bool IsStringAllowed(PrepareLogRecord prepareLogRecord);
		}

		private readonly IFilterStrategy _strategy;

		public EventFilter(TcpClientMessageDto.Filter[] filters) {
			if (filters == null || filters.Length == 0) {
				_strategy = new AlwaysAllowStrategy();
			} else {
				var strategies = new List<IFilterStrategy>();

				foreach (var filter in filters) {
					switch (filter.Context) {
						case TcpClientMessageDto.Filter.FilterContext.EventType
							when filter.Type == TcpClientMessageDto.Filter.FilterType.Prefix:
							strategies.Add(new EventTypePrefixStrategy(filter.Data));
							break;
						case TcpClientMessageDto.Filter.FilterContext.EventType
							when filter.Type == TcpClientMessageDto.Filter.FilterType.Regex:
							strategies.Add(new EventTypeRegexStrategy(filter.Data));
							break;
						case TcpClientMessageDto.Filter.FilterContext.StreamId
							when filter.Type == TcpClientMessageDto.Filter.FilterType.Prefix:
							strategies.Add(new StreamIdPrefixStrategy(filter.Data));
							break;
						case TcpClientMessageDto.Filter.FilterContext.StreamId
							when filter.Type == TcpClientMessageDto.Filter.FilterType.Regex:
							strategies.Add(new StreamIdRegexStrategy(filter.Data));
							break;
					}
				}

				_strategy = new MultiStrategyStrategy(strategies);
			}
		}

		private class AlwaysAllowStrategy : IFilterStrategy {
			public bool IsStringAllowed(PrepareLogRecord prepareLogRecord) {
				return true;
			}
		}


		private class StreamIdPrefixStrategy : IFilterStrategy {
			private readonly string _expectedPrefix;

			public StreamIdPrefixStrategy(string expectedPrefix) {
				_expectedPrefix = expectedPrefix;
			}

			public bool IsStringAllowed(PrepareLogRecord prepareLogRecord) {
				return prepareLogRecord.EventStreamId.StartsWith(_expectedPrefix);
			}
		}

		private class EventTypePrefixStrategy : IFilterStrategy {
			private readonly string _expectedPrefix;

			public EventTypePrefixStrategy(string expectedPrefix) {
				_expectedPrefix = expectedPrefix;
			}

			public bool IsStringAllowed(PrepareLogRecord prepareLogRecord) {
				return prepareLogRecord.EventType.StartsWith(_expectedPrefix);
			}
		}

		private class EventTypeRegexStrategy : IFilterStrategy {
			private readonly Regex _expectedRegex;

			public EventTypeRegexStrategy(string expectedRegex) {
				_expectedRegex = new Regex(expectedRegex, RegexOptions.Compiled);
			}

			public bool IsStringAllowed(PrepareLogRecord prepareLogRecord) {
				return _expectedRegex.Match(prepareLogRecord.EventType).Success;
			}
		}

		private class StreamIdRegexStrategy : IFilterStrategy {
			private readonly Regex _expectedRegex;

			public StreamIdRegexStrategy(string expectedRegex) {
				_expectedRegex = new Regex(expectedRegex, RegexOptions.Compiled);
			}

			public bool IsStringAllowed(PrepareLogRecord prepareLogRecord) {
				return _expectedRegex.Match(prepareLogRecord.EventStreamId).Success;
			}
		}

		public bool IsEventAllowed(PrepareLogRecord prepareLogRecord) {
			return _strategy.IsStringAllowed(prepareLogRecord);
		}

		private class MultiStrategyStrategy : IFilterStrategy {
			private readonly List<IFilterStrategy> _strategies;

			public MultiStrategyStrategy(List<IFilterStrategy> strategies) {
				_strategies = strategies;
			}

			public bool IsStringAllowed(PrepareLogRecord prepareLogRecord) {
				foreach (var strategy in _strategies) {
					if (strategy.IsStringAllowed(prepareLogRecord)) {
						return true;
					}
				}

				return false;
			}
		}
	}
}
