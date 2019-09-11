using System.Collections.Generic;
using System.Text.RegularExpressions;
using EventStore.ClientAPI.Common.Utils;
using EventStore.ClientAPI.Messages;

namespace EventStore.ClientAPI {
	
	/// <summary>
	/// Creates a filter to be applied on the server whilst reading the all stream. 
	/// </summary>
	/// <remarks>
	/// Any filters appended will be applied as or statements. That is to say if two event type filters are applied an
	/// event will be returned if either of them match.
	/// </remarks>
	public sealed class EventFilter {
		internal readonly ClientMessage.Filter[] Filters;

		internal EventFilter(ClientMessage.Filter[] filters) {
			Filters = filters;
		}

		/// <summary>
		/// Create a new <see cref="EventFilterBuilder"/>
		/// </summary>
		/// <returns>A new <see cref="EventFilterBuilder" /></returns>
		public static EventFilterBuilder Create() {
			return new EventFilterBuilder();
		}
	}

	/// <summary>
	/// This class is responsible for building an <see cref="EventFilter"/>
	/// </summary>
	public class EventFilterBuilder {
		private readonly List<ClientMessage.Filter> _filters;

		internal EventFilterBuilder() {
			_filters = new List<ClientMessage.Filter>();
		}

		/// <summary>
		/// Add a filter that will filter events whose event type match a given prefix.
		/// </summary>
		/// <param name="eventTypePrefixFilter">The prefix to filter by.</param>
		public EventFilterBuilder WithEventTypePrefixFilter(string eventTypePrefixFilter) {
			Ensure.NotNull(eventTypePrefixFilter.GetType(), nameof(eventTypePrefixFilter));
			_filters.Add(new ClientMessage.Filter(
				ClientMessage.Filter.FilterContext.EventType,
				ClientMessage.Filter.FilterType.Prefix,
				eventTypePrefixFilter));
			return this;
		}

		/// <summary>
		/// Add a filter that will filter events whose event type match a given regular expression.
		/// </summary>
		/// <param name="eventTypeFilter">A regular expression to filter by.</param>
		public EventFilterBuilder WithEventTypeFilter(Regex eventTypeFilter) {
			Ensure.NotNull(eventTypeFilter, nameof(eventTypeFilter));
			_filters.Add(new ClientMessage.Filter(
				ClientMessage.Filter.FilterContext.EventType,
				ClientMessage.Filter.FilterType.Regex,
				eventTypeFilter.ToString()));
			return this;
		}

		/// <summary>
		/// Add a filter that will filter events whose stream id match a given prefix.
		/// </summary>
		/// <param name="streamIdPrefixFilter">The prefix to filter by.</param>
		public EventFilterBuilder WithStreamIdPrefixFilter(string streamIdPrefixFilter) {
			Ensure.NotNull(streamIdPrefixFilter, nameof(streamIdPrefixFilter));
			_filters.Add(new ClientMessage.Filter(
				ClientMessage.Filter.FilterContext.StreamId,
				ClientMessage.Filter.FilterType.Prefix,
				streamIdPrefixFilter));
			return this;
		}

		/// <summary>
		/// Add a filter that will filter events whose stream id match a given regular expression.
		/// </summary>
		/// <param name="streamIdFilter">A regular expression to filter by.</param>
		public EventFilterBuilder WithStreamIdFilter(Regex streamIdFilter) {
			Ensure.NotNull(streamIdFilter, nameof(streamIdFilter));
			_filters.Add(new ClientMessage.Filter(
				ClientMessage.Filter.FilterContext.StreamId,
				ClientMessage.Filter.FilterType.Regex,
				streamIdFilter.ToString()));
			return this;
		}

		/// <summary>
		/// Excludes all systems events. These are any that begin with $.
		/// </summary>
		public EventFilterBuilder ExcludeSystemEvents() {
			WithEventTypeFilter(new Regex(@"^[^\$].*"));
			return this;
		}

		/// <summary>
		/// Build the event filter.
		/// </summary>
		/// <returns>A <see cref="EventFilter"/></returns>
		public EventFilter Build() {
			return new EventFilter(_filters.ToArray());
		}
	}
}
