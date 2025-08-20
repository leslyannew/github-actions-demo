using OpenTelemetry;
using OpenTelemetry.Logs;

namespace github_actions_demo.Setup;
internal sealed class NewRelicLogProcessor : BaseProcessor<LogRecord>
{
    public override void OnEnd(LogRecord logRecord)
    {
        var attributes = new List<KeyValuePair<string, object?>>()
        {
            //per new relic documentation
            //to correlate traces with logs
            //https://docs.newrelic.com/docs/more-integrations/open-source-telemetry-integrations/opentelemetry/view-your-data/opentelemetry-logs-page/
             new KeyValuePair<string, object?>("trace.id", logRecord.TraceId.ToString()),
             new KeyValuePair<string, object?>("span.id", logRecord.SpanId.ToString() ),
        };

        if (logRecord.Attributes != null)
        {
            attributes.AddRange(logRecord.Attributes.ToList());
        }
        logRecord.Attributes = attributes;
    }
}
