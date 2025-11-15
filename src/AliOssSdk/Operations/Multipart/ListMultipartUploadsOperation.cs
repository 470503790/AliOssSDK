using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Xml.Linq;
using AliOssSdk.Http;
using AliOssSdk.Models.Multipart;

namespace AliOssSdk.Operations.Multipart
{
    public sealed class ListMultipartUploadsOperation : IOssOperation<ListMultipartUploadsResponse>
    {
        private readonly ListMultipartUploadsRequest _request;

        public ListMultipartUploadsOperation(ListMultipartUploadsRequest request)
        {
            _request = request ?? throw new ArgumentNullException(nameof(request));
        }

        public string Name => "ListMultipartUploads";

        public OssHttpRequest BuildRequest(OssOperationContext context)
        {
            var resource = $"/{_request.BucketName}";
            var httpRequest = new OssHttpRequest(HttpMethod.Get, resource);
            httpRequest.QueryParameters["uploads"] = string.Empty;

            if (!string.IsNullOrWhiteSpace(_request.Prefix))
            {
                httpRequest.QueryParameters["prefix"] = _request.Prefix;
            }

            if (!string.IsNullOrWhiteSpace(_request.Delimiter))
            {
                httpRequest.QueryParameters["delimiter"] = _request.Delimiter;
            }

            if (!string.IsNullOrWhiteSpace(_request.KeyMarker))
            {
                httpRequest.QueryParameters["key-marker"] = _request.KeyMarker;
            }

            if (!string.IsNullOrWhiteSpace(_request.UploadIdMarker))
            {
                httpRequest.QueryParameters["upload-id-marker"] = _request.UploadIdMarker;
            }

            if (_request.MaxUploads is int maxUploads)
            {
                httpRequest.QueryParameters["max-uploads"] = maxUploads.ToString(CultureInfo.InvariantCulture);
            }

            return httpRequest;
        }

        public ListMultipartUploadsResponse ParseResponse(OssHttpResponse response)
        {
            try
            {
                var document = XDocument.Load(response.ContentStream);
                var ns = document.Root?.GetDefaultNamespace() ?? XNamespace.None;
                var uploads = new List<MultipartUploadSummary>();
                foreach (var element in document.Descendants(ns + "Upload"))
                {
                    uploads.Add(new MultipartUploadSummary
                    {
                        Key = element.Element(ns + "Key")?.Value,
                        UploadId = element.Element(ns + "UploadId")?.Value,
                        Initiated = TryParseDate(element.Element(ns + "Initiated")?.Value)
                    });
                }

                var truncated = string.Equals(document.Root?.Element(ns + "IsTruncated")?.Value, "true", StringComparison.OrdinalIgnoreCase);
                return new ListMultipartUploadsResponse
                {
                    Uploads = uploads,
                    IsTruncated = truncated,
                    NextKeyMarker = document.Root?.Element(ns + "NextKeyMarker")?.Value,
                    NextUploadIdMarker = document.Root?.Element(ns + "NextUploadIdMarker")?.Value
                };
            }
            catch
            {
                return new ListMultipartUploadsResponse();
            }
        }

        private static DateTimeOffset? TryParseDate(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsed)
                ? parsed
                : (DateTimeOffset?)null;
        }
    }
}
