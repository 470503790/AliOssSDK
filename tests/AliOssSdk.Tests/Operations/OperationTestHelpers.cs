using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using AliOssSdk.Configuration;
using AliOssSdk.Http;
using AliOssSdk.Operations;

namespace AliOssSdk.Tests.Operations
{

    internal static class OperationTestHelpers
    {
        public static OssOperationContext CreateContext(string? defaultRegion = null, string? defaultBucket = null, string? endpoint = null)
        {
            var endpointUri = endpoint != null ? new Uri(endpoint) : new Uri("https://oss.example.com");
            var configuration = new OssClientConfiguration(endpointUri, "key", "secret")
            {
                DefaultRegion = defaultRegion,
                DefaultBucketName = defaultBucket
            };
            return new OssOperationContext(configuration);
        }

        public static OssHttpResponse CreateResponse(string payload, HttpStatusCode statusCode = HttpStatusCode.OK, IReadOnlyDictionary<string, string>? headers = null)
        {
            var buffer = Encoding.UTF8.GetBytes(payload ?? string.Empty);
            var stream = new MemoryStream(buffer);
            return new OssHttpResponse(statusCode, stream, headers ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
        }

        public static OssHttpResponse CreateResponse(Stream payload, HttpStatusCode statusCode = HttpStatusCode.OK, IReadOnlyDictionary<string, string>? headers = null)
        {
            if (payload.CanSeek)
            {
                payload.Position = 0;
            }

            return new OssHttpResponse(statusCode, payload, headers ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
        }

        public static IReadOnlyDictionary<string, string> Headers(params (string Key, string Value)[] headers)
        {
            return headers.ToDictionary(h => h.Key, h => h.Value, StringComparer.OrdinalIgnoreCase);
        }
    }
}
