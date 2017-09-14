using Serilog.Core;
using Serilog.Events;
using System;

namespace MSBuildProjectTools.LanguageServer.Logging
{
    using Utilities;

    /// <summary>
    ///     Serilog event enricher that adds the current logical activity Id.
    /// </summary>
    class ActivityIdEnricher
        : ILogEventEnricher
    {
        /// <summary>
        ///     Enrich the specified <see cref="LogEvent"/> with the current activity Id (if any).
        /// </summary>
        /// <param name="logEvent">
        ///     The <see cref="LogEvent"/> to enrich.
        /// </param>
        /// <param name="propertyFactory">
        ///     The factory for log event properties.
        /// </param>
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (logEvent == null)
                throw new ArgumentNullException(nameof(logEvent));
            
            if (propertyFactory == null)
                throw new ArgumentNullException(nameof(propertyFactory));
            
            Guid? activityId = ActivityCorrelationManager.CurrentActivityId;
            LogEventProperty activityIdProperty = propertyFactory.CreateProperty("ActivityId", activityId);
            logEvent.AddPropertyIfAbsent(activityIdProperty);
        }
    }
}
