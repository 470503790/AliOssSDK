using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Xml.Linq;
using AliOssSdk.Http;
using AliOssSdk.Models.Buckets;

namespace AliOssSdk.Operations.Buckets
{
    public sealed class ListBucketsOperation : IOssOperation<ListBucketsResponse>
    {
        private readonly ListBucketsRequest _request;

        public ListBucketsOperation(ListBucketsRequest request)
        {
            _request = request ?? throw new ArgumentNullException(nameof(request));
        }

        public string Name => "ListBuckets";

        public OssHttpRequest BuildRequest(OssOperationContext context)
        {
            var httpRequest = new OssHttpRequest(HttpMethod.Get, "/");
            if (!string.IsNullOrWhiteSpace(_request.Prefix))
            {
                httpRequest.QueryParameters["prefix"] = _request.Prefix;
            }

            if (_request.MaxKeys is int maxKeys)
            {
                httpRequest.QueryParameters["max-keys"] = maxKeys.ToString();
            }

            return httpRequest;
        }

        public ListBucketsResponse ParseResponse(OssHttpResponse response)
        {
            var buckets = new List<string>();
            try
            {
                var document = XDocument.Load(response.ContentStream);
                buckets.AddRange(document.Descendants("Bucket").Select(e => e.Element("Name")?.Value).Where(name => !string.IsNullOrWhiteSpace(name))!);
                var nextMarker = document.Root?.Element("NextMarker")?.Value;
                return new ListBucketsResponse
                {
                    Buckets = buckets,
                    NextMarker = string.IsNullOrWhiteSpace(nextMarker) ? null : nextMarker
                };
            }
            catch
            {
                return new ListBucketsResponse { Buckets = buckets };
            }
        }
    }
}
