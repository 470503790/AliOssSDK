using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Xml.Linq;
using AliOssSdk.Http;
using AliOssSdk.Models.Multipart;

namespace AliOssSdk.Operations.Multipart
{
    public sealed class ListPartsOperation : IOssOperation<ListPartsResponse>
    {
        private readonly ListPartsRequest _request;

        public ListPartsOperation(ListPartsRequest request)
        {
            _request = request ?? throw new ArgumentNullException(nameof(request));
        }

        public string Name => "ListParts";

        public OssHttpRequest BuildRequest(OssOperationContext context)
        {
            var bucket = context.ResolveBucketName(_request.BucketName);
            var resource = context.BuildResourcePath(bucket, _request.ObjectKey);
            var httpRequest = new OssHttpRequest(HttpMethod.Get, resource);
            httpRequest.QueryParameters["uploadId"] = _request.UploadId;

            if (_request.PartNumberMarker is int marker)
            {
                httpRequest.QueryParameters["part-number-marker"] = marker.ToString(CultureInfo.InvariantCulture);
            }

            if (_request.MaxParts is int maxParts)
            {
                httpRequest.QueryParameters["max-parts"] = maxParts.ToString(CultureInfo.InvariantCulture);
            }

            return httpRequest;
        }

        public ListPartsResponse ParseResponse(OssHttpResponse response)
        {
            try
            {
                var document = XDocument.Load(response.ContentStream);
                var ns = document.Root?.GetDefaultNamespace() ?? XNamespace.None;
                var parts = new List<MultipartPartSummary>();
                foreach (var element in document.Descendants(ns + "Part"))
                {
                    parts.Add(new MultipartPartSummary
                    {
                        PartNumber = (int)TryParseLong(element.Element(ns + "PartNumber")?.Value),
                        ETag = element.Element(ns + "ETag")?.Value?.Trim('"'),
                        Size = TryParseLong(element.Element(ns + "Size")?.Value),
                        LastModified = TryParseDate(element.Element(ns + "LastModified")?.Value)
                    });
                }

                var truncated = string.Equals(document.Root?.Element(ns + "IsTruncated")?.Value, "true", StringComparison.OrdinalIgnoreCase);
                var nextMarkerValue = document.Root?.Element(ns + "NextPartNumberMarker")?.Value;
                int? nextMarker = null;
                if (int.TryParse(nextMarkerValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedMarker))
                {
                    nextMarker = parsedMarker;
                }

                return new ListPartsResponse
                {
                    Parts = parts,
                    IsTruncated = truncated,
                    NextPartNumberMarker = nextMarker
                };
            }
            catch
            {
                return new ListPartsResponse();
            }
        }

        private static long TryParseLong(string? value) => long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) ? parsed : 0L;

        private static DateTimeOffset? TryParseDate(string? value) => DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsed) ? parsed : null;
    }
}
