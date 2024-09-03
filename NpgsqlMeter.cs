using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Diagnostics.Tracing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace NpgsqlCommandRaceSample;

public class NpgSqlMeter : EventListener
    {
        private const string EventSourceName = "Npgsql";

        private readonly ConcurrentDictionary<string, Measurement<double>> _values = new();

        public static string Name => "Npgsql";
        private static readonly Meter Meter = new(Name);

        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            if (eventSource.Name != EventSourceName)
            {
                return;
            }

            EnableEvents(eventSource, EventLevel.LogAlways, EventKeywords.All,
                new Dictionary<string, string> { ["EventCounterIntervalSec"] = "1" });

            InitMetrics();
        }

        private void InitMetrics()
        {
            Meter.CreateObservableGauge($"prefix_rdbms_active_connections",
                () => _values.GetValueOrDefault("busy-connections"));

        }

        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            if (eventData.EventSource.Name != EventSourceName
                || eventData.EventName != "EventCounters"
                || eventData.Payload == null)
            {
                return;
            }

            SaveMeasurements(eventData.Payload);
        }

        private void SaveMeasurements(ReadOnlyCollection<object> payload)
        {
            foreach (var payloadItem in payload)
            {
                if (payloadItem is not IDictionary<string, object> payloadDict)
                {
                    continue;
                }

                if (!payloadDict.TryGetValue("Name", out var name)
                    || !(payloadDict.TryGetValue("Mean", out var value)
                         || payloadDict.TryGetValue("Increment", out value)))
                {
                    continue;
                }

                _values[(string)name] =
                    new Measurement<double>((double)value);
            }
        }
    }
