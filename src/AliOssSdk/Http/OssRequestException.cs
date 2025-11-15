using System;
using System.Collections.Generic;
using System.Net;

namespace AliOssSdk.Http
{
    /// <summary>
    /// Represents an OSS response with a non-success HTTP status code.
    /// </summary>
    public sealed class OssRequestException : Exception
    {
        public OssRequestException(OssHttpResponse response, string? responseBody)
            : base(CreateMessage(response, responseBody))
        {
            Response = response ?? throw new ArgumentNullException(nameof(response));
            StatusCode = response.StatusCode;
            RequestId = response.RequestId;
            ResponseBody = responseBody;
            ResponseHeaders = CreateResponseHeaders(response.Headers);
        }

        public HttpStatusCode StatusCode { get; }

        public string? RequestId { get; }

        public string? ResponseBody { get; }

        public IReadOnlyDictionary<string, string> ResponseHeaders { get; }

        public OssHttpResponse Response { get; }

        private static IReadOnlyDictionary<string, string> CreateResponseHeaders(IReadOnlyDictionary<string, string> headers)
        {
            if (headers == null)
            {
                throw new ArgumentNullException(nameof(headers));
            }

            var dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var header in headers)
            {
                dictionary[header.Key] = header.Value;
            }

            return dictionary;
        }

        private static string CreateMessage(OssHttpResponse response, string? responseBody)
        {
            if (response == null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            var requestIdSegment = string.IsNullOrWhiteSpace(response.RequestId)
                ? string.Empty
                : $" (RequestId: {response.RequestId})";
            var bodySegment = string.IsNullOrWhiteSpace(responseBody)
                ? string.Empty
                : $" Body: {responseBody}";
            return $"OSS request failed with status {(int)response.StatusCode} {response.StatusCode}{requestIdSegment}.{bodySegment}";
        }
    }
}
