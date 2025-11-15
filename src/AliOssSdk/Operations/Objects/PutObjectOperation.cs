using System;
using System.Net.Http;
using AliOssSdk.Http;
using AliOssSdk.Models.Objects;

namespace AliOssSdk.Operations.Objects
{
    public sealed class PutObjectOperation : IOssOperation<PutObjectResponse>
    {
        private readonly PutObjectRequest _request;

        public PutObjectOperation(PutObjectRequest request)
        {
            _request = request ?? throw new ArgumentNullException(nameof(request));
        }

        public string Name => "PutObject";

        public OssHttpRequest BuildRequest(OssOperationContext context)
        {
            var resource = $"/{_request.BucketName}/{_request.ObjectKey}";
            return new OssHttpRequest(HttpMethod.Put, resource)
            {
                Content = _request.Content,
                ContentType = _request.ContentType ?? "application/octet-stream"
            };
        }

        public PutObjectResponse ParseResponse(OssHttpResponse response) => new PutObjectResponse
        {
            StatusCode = response.StatusCode,
            ETag = response.Headers.TryGetValue("ETag", out var etag) ? etag : null
        };
    }
}
