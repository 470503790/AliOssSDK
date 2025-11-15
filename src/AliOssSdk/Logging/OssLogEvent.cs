using System;
using System.Collections.Generic;

namespace AliOssSdk.Logging
{
    /// <summary>
    /// Represents a structured log event emitted by the OSS client.
    /// </summary>
    public sealed class OssLogEvent
    {
        public OssLogEvent(OssLogEventType eventType, string operationName, Guid invocationId, int attempt,
            IReadOnlyDictionary<string, object?>? data, Exception? exception)
        {
            EventType = eventType;
            OperationName = operationName;
            InvocationId = invocationId;
            Attempt = attempt;
            Data = data;
            Exception = exception;
            Timestamp = DateTimeOffset.UtcNow;
        }

        public OssLogEventType EventType { get; }

        public string OperationName { get; }

        public Guid InvocationId { get; }

        public int Attempt { get; }

        public IReadOnlyDictionary<string, object?>? Data { get; }

        public Exception? Exception { get; }

        public DateTimeOffset Timestamp { get; }

        public static OssLogEvent RequestStart(string operationName, Guid invocationId, int attempt,
            IReadOnlyDictionary<string, object?> data) =>
            new(OssLogEventType.RequestStart, operationName, invocationId, attempt, data, null);

        public static OssLogEvent RequestHeaders(string operationName, Guid invocationId, int attempt,
            IReadOnlyDictionary<string, object?> data) =>
            new(OssLogEventType.RequestHeaders, operationName, invocationId, attempt, data, null);

        public static OssLogEvent RequestBody(string operationName, Guid invocationId, int attempt,
            IReadOnlyDictionary<string, object?> data) =>
            new(OssLogEventType.RequestBody, operationName, invocationId, attempt, data, null);

        public static OssLogEvent Retry(string operationName, Guid invocationId, int attempt, bool isRetry,
            Exception? lastException)
        {
            var payload = new Dictionary<string, object?>
            {
                ["Attempt"] = attempt,
                ["IsRetry"] = isRetry
            };

            return new(OssLogEventType.Retry, operationName, invocationId, attempt, payload, lastException);
        }

        public static OssLogEvent Response(string operationName, Guid invocationId, int attempt,
            IReadOnlyDictionary<string, object?> data) =>
            new(OssLogEventType.Response, operationName, invocationId, attempt, data, null);

        public static OssLogEvent Error(string operationName, Guid invocationId, int attempt,
            IReadOnlyDictionary<string, object?>? data, Exception exception) =>
            new(OssLogEventType.Error, operationName, invocationId, attempt, data, exception);
    }
}
