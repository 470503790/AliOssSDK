using System;
using System.Globalization;
using System.Linq;

namespace AliOssSdk.Logging
{
    /// <summary>
    /// Writes structured events to the console for quick diagnostics.
    /// </summary>
    public sealed class ConsoleLogger : ILogger
    {
        private readonly object _lock = new object();

        public void Log(OssLogEvent logEvent)
        {
            if (logEvent == null)
            {
                return;
            }

            var data = logEvent.Data == null || logEvent.Data.Count == 0
                ? string.Empty
                : string.Join(", ", logEvent.Data.Select(p => $"{p.Key}={p.Value}"));

            var message = string.Format(CultureInfo.InvariantCulture,
                "{0:o} [{1}] op={2} attempt={3} id={4} {5}",
                logEvent.Timestamp,
                logEvent.EventType,
                logEvent.OperationName,
                logEvent.Attempt,
                logEvent.InvocationId,
                data);

            if (logEvent.Exception != null)
            {
                message += $" Exception: {logEvent.Exception}";
            }

            lock (_lock)
            {
                Console.WriteLine(message.Trim());
            }
        }
    }
}
