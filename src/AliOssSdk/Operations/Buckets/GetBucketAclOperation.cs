using System;
using System.Net.Http;
using System.Xml.Linq;
using AliOssSdk.Http;
using AliOssSdk.Models.Buckets;

namespace AliOssSdk.Operations.Buckets
{
    public sealed class GetBucketAclOperation : IOssOperation<GetBucketAclResponse>
    {
        private readonly GetBucketAclRequest _request;

        public GetBucketAclOperation(GetBucketAclRequest request)
        {
            _request = request ?? throw new ArgumentNullException(nameof(request));
        }

        public string Name => "GetBucketAcl";

        public OssHttpRequest BuildRequest(OssOperationContext context)
        {
            var resource = $"/{_request.BucketName}";
            var httpRequest = new OssHttpRequest(HttpMethod.Get, resource);
            httpRequest.QueryParameters["acl"] = string.Empty;
            return httpRequest;
        }

        public GetBucketAclResponse ParseResponse(OssHttpResponse response)
        {
            try
            {
                var document = XDocument.Load(response.ContentStream);
                var owner = document.Root?.Element("Owner");
                var grant = document.Root?.Element("AccessControlList")?.Element("Grant")?.Value;
                return new GetBucketAclResponse
                {
                    OwnerId = owner?.Element("ID")?.Value,
                    OwnerDisplayName = owner?.Element("DisplayName")?.Value,
                    Grant = grant
                };
            }
            catch
            {
                return new GetBucketAclResponse();
            }
        }
    }
}
