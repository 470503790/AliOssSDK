using System;
using System.Net.Http;
using AliOssSdk.Http;
using AliOssSdk.Models.Objects;

namespace AliOssSdk.Operations.Objects
{
    public sealed class GetObjectOperation : IOssOperation<GetObjectResponse>
    {
        private readonly GetObjectRequest _request;

        public GetObjectOperation(GetObjectRequest request)
        {
            _request = request ?? throw new ArgumentNullException(nameof(request));
        }

        public string Name => "GetObject";

        public OssHttpRequest BuildRequest(OssOperationContext context)
        {
            var resource = $"/{_request.BucketName}/{_request.ObjectKey}";
            return new OssHttpRequest(HttpMethod.Get, resource);
        }

        public GetObjectResponse ParseResponse(OssHttpResponse response) => new()
        {
            StatusCode = response.StatusCode,
            Content = response.ContentStream,
            ContentType = response.Headers.TryGetValue("Content-Type", out var contentType) ? contentType : null
        };
    }
}
