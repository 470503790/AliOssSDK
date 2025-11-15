using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using AliOssSdk.Http;
using AliOssSdk.Models.Objects;

namespace AliOssSdk.Operations.Objects
{
    public sealed class HeadObjectOperation : IOssOperation<HeadObjectResponse>
    {
        private readonly HeadObjectRequest _request;

        public HeadObjectOperation(HeadObjectRequest request)
        {
            _request = request ?? throw new ArgumentNullException(nameof(request));
        }

        public string Name => "HeadObject";

        public OssHttpRequest BuildRequest(OssOperationContext context)
        {
            var resource = $"/{_request.BucketName}/{_request.ObjectKey}";
            return new OssHttpRequest(HttpMethod.Head, resource);
        }

        public HeadObjectResponse ParseResponse(OssHttpResponse response)
        {
            var metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var header in response.Headers)
            {
                if (header.Key.StartsWith("x-oss-meta-", StringComparison.OrdinalIgnoreCase))
                {
                    metadata[header.Key] = header.Value;
                }
            }

            return new HeadObjectResponse
            {
                StatusCode = response.StatusCode,
                ContentLength = TryParseLong(response.Headers.TryGetValue("Content-Length", out var length) ? length : null),
                ContentType = response.Headers.TryGetValue("Content-Type", out var type) ? type : null,
                LastModified = TryParseDate(response.Headers.TryGetValue("Last-Modified", out var modified) ? modified : null),
                ETag = response.Headers.TryGetValue("ETag", out var etag) ? etag : null,
                Metadata = metadata
            };
        }

        private static long? TryParseLong(string? value) => long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) ? parsed : null;

        private static DateTimeOffset? TryParseDate(string? value) => DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsed) ? parsed : null;
    }
}
