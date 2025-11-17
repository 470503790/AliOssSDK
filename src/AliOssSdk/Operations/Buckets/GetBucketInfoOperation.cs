using System;
using System.Globalization;
using System.Net.Http;
using System.Xml.Linq;
using AliOssSdk.Http;
using AliOssSdk.Models.Buckets;

namespace AliOssSdk.Operations.Buckets
{
    public sealed class GetBucketInfoOperation : IOssOperation<GetBucketInfoResponse>
    {
        private readonly GetBucketInfoRequest _request;

        public GetBucketInfoOperation(GetBucketInfoRequest request)
        {
            _request = request ?? throw new ArgumentNullException(nameof(request));
        }

        public string Name => "GetBucketInfo";

        public OssHttpRequest BuildRequest(OssOperationContext context)
        {
            var bucket = context.ResolveBucketName(_request.BucketName);
            var resource = context.BuildBucketResourcePath(bucket);
            var httpRequest = new OssHttpRequest(HttpMethod.Get, resource);
            httpRequest.QueryParameters["bucketInfo"] = string.Empty;
            return httpRequest;
        }

        public GetBucketInfoResponse ParseResponse(OssHttpResponse response)
        {
            try
            {
                var document = XDocument.Load(response.ContentStream);
                var bucket = document.Root?.Element("Bucket");
                if (bucket == null)
                {
                    return new GetBucketInfoResponse();
                }

                DateTimeOffset? creationDate = null;
                var creation = bucket.Element("CreationDate")?.Value;
                if (!string.IsNullOrWhiteSpace(creation) && DateTimeOffset.TryParse(creation, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsed))
                {
                    creationDate = parsed;
                }

                return new GetBucketInfoResponse
                {
                    Name = bucket.Element("Name")?.Value,
                    Location = bucket.Element("Location")?.Value,
                    CreationDate = creationDate
                };
            }
            catch
            {
                return new GetBucketInfoResponse();
            }
        }
    }
}
