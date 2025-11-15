using System;
using System.Net.Http;
using AliOssSdk.Http;
using AliOssSdk.Models.Buckets;

namespace AliOssSdk.Operations.Buckets
{
    public sealed class DeleteBucketOperation : IOssOperation<DeleteBucketResponse>
    {
        private readonly DeleteBucketRequest _request;

        public DeleteBucketOperation(DeleteBucketRequest request)
        {
            _request = request ?? throw new ArgumentNullException(nameof(request));
        }

        public string Name => "DeleteBucket";

        public OssHttpRequest BuildRequest(OssOperationContext context)
        {
            var resource = $"/{_request.BucketName}";
            return new OssHttpRequest(HttpMethod.Delete, resource);
        }

        public DeleteBucketResponse ParseResponse(OssHttpResponse response) => new()
        {
            StatusCode = response.StatusCode
        };
    }
}
