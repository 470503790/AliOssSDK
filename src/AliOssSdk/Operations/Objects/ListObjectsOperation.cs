using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Xml.Linq;
using AliOssSdk.Http;
using AliOssSdk.Models.Objects;

namespace AliOssSdk.Operations.Objects
{
    public sealed class ListObjectsOperation : IOssOperation<ListObjectsResponse>
    {
        private readonly ListObjectsRequest _request;

        public ListObjectsOperation(ListObjectsRequest request)
        {
            _request = request ?? throw new ArgumentNullException(nameof(request));
        }

        public string Name => "ListObjects";

        public OssHttpRequest BuildRequest(OssOperationContext context)
        {
            var bucket = context.ResolveBucketName(_request.BucketName);
            var resource = context.BuildResourcePath(bucket, "");
            var httpRequest = new OssHttpRequest(HttpMethod.Get, resource);

            if (!string.IsNullOrWhiteSpace(_request.Prefix))
            {
                httpRequest.QueryParameters["prefix"] = _request.Prefix;
            }

            if (!string.IsNullOrWhiteSpace(_request.Delimiter))
            {
                httpRequest.QueryParameters["delimiter"] = _request.Delimiter;
            }

            if (!string.IsNullOrWhiteSpace(_request.Marker))
            {
                httpRequest.QueryParameters["marker"] = _request.Marker;
            }

            if (_request.MaxKeys is int maxKeys)
            {
                httpRequest.QueryParameters["max-keys"] = maxKeys.ToString(CultureInfo.InvariantCulture);
            }

            return httpRequest;
        }

        public ListObjectsResponse ParseResponse(OssHttpResponse response)
        {
            try
            {
                var document = XDocument.Load(response.ContentStream);
                var ns = document.Root?.GetDefaultNamespace() ?? XNamespace.None;
                var objects = new List<ObjectSummary>();
                foreach (var element in document.Descendants(ns + "Contents"))
                {
                    var summary = new ObjectSummary
                    {
                        Key = element.Element(ns + "Key")?.Value ?? string.Empty,
                        ETag = element.Element(ns + "ETag")?.Value?.Trim('"'),
                        Size = TryParseLong(element.Element(ns + "Size")?.Value),
                        LastModified = TryParseDate(element.Element(ns + "LastModified")?.Value)
                    };
                    objects.Add(summary);
                }

                var nextMarker = document.Root?.Element(ns + "NextMarker")?.Value;
                var isTruncated = string.Equals(document.Root?.Element(ns + "IsTruncated")?.Value, "true", StringComparison.OrdinalIgnoreCase);

                return new ListObjectsResponse
                {
                    Objects = objects,
                    NextMarker = string.IsNullOrWhiteSpace(nextMarker) ? null : nextMarker,
                    IsTruncated = isTruncated
                };
            }
            catch
            {
                return new ListObjectsResponse();
            }
        }

        private static long TryParseLong(string? value) => long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) ? parsed : 0L;

        private static DateTimeOffset? TryParseDate(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsed)
                ? parsed
                : null;
        }
    }
}
