using System;
using System.Net.Http;
using AliOssSdk.Http;
using AliOssSdk.Models.Buckets;

namespace AliOssSdk.Operations.Buckets
{
    public sealed class PutBucketAclOperation : IOssOperation<PutBucketAclResponse>
    {
        private readonly PutBucketAclRequest _request;

        public PutBucketAclOperation(PutBucketAclRequest request)
        {
            _request = request ?? throw new ArgumentNullException(nameof(request));
        }

        public string Name => "PutBucketAcl";

        public OssHttpRequest BuildRequest(OssOperationContext context)
        {
            var resource = $"/{_request.BucketName}";
            var httpRequest = new OssHttpRequest(HttpMethod.Put, resource);
            httpRequest.QueryParameters["acl"] = string.Empty;
            httpRequest.Headers["x-oss-acl"] = ToHeaderValue(_request.Acl);
            return httpRequest;
        }

        public PutBucketAclResponse ParseResponse(OssHttpResponse response) => new PutBucketAclResponse
        {
            StatusCode = response.StatusCode
        };

        private static string ToHeaderValue(BucketAcl acl) => acl switch
        {
            BucketAcl.PublicRead => "public-read",
            BucketAcl.PublicReadWrite => "public-read-write",
            _ => "private"
        };
    }
}
