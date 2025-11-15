using System;
using System.Net.Http;
using AliOssSdk.Models.Buckets;
using AliOssSdk.Http;

namespace AliOssSdk.Operations.Buckets
{
    public sealed class CreateBucketOperation : IOssOperation<CreateBucketResponse>
    {
        private readonly CreateBucketRequest _request;

        public CreateBucketOperation(CreateBucketRequest request)
        {
            _request = request ?? throw new ArgumentNullException(nameof(request));
        }

        public string Name => "CreateBucket";

        public OssHttpRequest BuildRequest(OssOperationContext context)
        {
            var resource = $"/{_request.BucketName}/";
            var httpRequest = new OssHttpRequest(HttpMethod.Put, resource);

            var region = _request.Region ?? context.Configuration.DefaultRegion;
            if (!string.IsNullOrWhiteSpace(region))
            {
                httpRequest.Headers["x-oss-bucket-region"] = region;
            }

            return httpRequest;
        }

        public CreateBucketResponse ParseResponse(OssHttpResponse response) => new CreateBucketResponse
        {
            StatusCode = response.StatusCode,
            Location = response.Headers.TryGetValue("Location", out var location) ? location : null
        };
    }
}
