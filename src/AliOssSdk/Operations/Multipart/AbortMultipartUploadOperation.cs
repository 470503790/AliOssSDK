using System;
using System.Net.Http;
using AliOssSdk.Http;
using AliOssSdk.Models.Multipart;

namespace AliOssSdk.Operations.Multipart
{
    public sealed class AbortMultipartUploadOperation : IOssOperation<AbortMultipartUploadResponse>
    {
        private readonly AbortMultipartUploadRequest _request;

        public AbortMultipartUploadOperation(AbortMultipartUploadRequest request)
        {
            _request = request ?? throw new ArgumentNullException(nameof(request));
        }

        public string Name => "AbortMultipartUpload";

        public OssHttpRequest BuildRequest(OssOperationContext context)
        {
            var bucket = context.ResolveBucketName(_request.BucketName);
            var resource = $"/{bucket}/{_request.ObjectKey}";
            var httpRequest = new OssHttpRequest(HttpMethod.Delete, resource);
            httpRequest.QueryParameters["uploadId"] = _request.UploadId;
            return httpRequest;
        }

        public AbortMultipartUploadResponse ParseResponse(OssHttpResponse response) => new AbortMultipartUploadResponse
        {
            StatusCode = response.StatusCode
        };
    }
}
