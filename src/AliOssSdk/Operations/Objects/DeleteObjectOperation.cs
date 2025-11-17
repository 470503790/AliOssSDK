using System;
using System.Net.Http;
using AliOssSdk.Http;
using AliOssSdk.Models.Objects;

namespace AliOssSdk.Operations.Objects
{
    public sealed class DeleteObjectOperation : IOssOperation<DeleteObjectResponse>
    {
        private readonly DeleteObjectRequest _request;

        public DeleteObjectOperation(DeleteObjectRequest request)
        {
            _request = request ?? throw new ArgumentNullException(nameof(request));
        }

        public string Name => "DeleteObject";

        public OssHttpRequest BuildRequest(OssOperationContext context)
        {
            var bucket = context.ResolveBucketName(_request.BucketName);
            var resource = context.BuildResourcePath(bucket, _request.ObjectKey);
            return new OssHttpRequest(HttpMethod.Delete, resource);
        }

        public DeleteObjectResponse ParseResponse(OssHttpResponse response) => new DeleteObjectResponse
        {
            StatusCode = response.StatusCode
        };
    }
}
