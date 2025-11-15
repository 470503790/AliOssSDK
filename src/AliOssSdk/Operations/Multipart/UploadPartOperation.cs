using System;
using System.Globalization;
using System.Net.Http;
using AliOssSdk.Http;
using AliOssSdk.Models.Multipart;

namespace AliOssSdk.Operations.Multipart
{
    public sealed class UploadPartOperation : IOssOperation<UploadPartResponse>
    {
        private readonly UploadPartRequest _request;

        public UploadPartOperation(UploadPartRequest request)
        {
            _request = request ?? throw new ArgumentNullException(nameof(request));
        }

        public string Name => "UploadPart";

        public OssHttpRequest BuildRequest(OssOperationContext context)
        {
            var resource = $"/{_request.BucketName}/{_request.ObjectKey}";
            var httpRequest = new OssHttpRequest(HttpMethod.Put, resource)
            {
                Content = _request.Content,
                ContentType = _request.ContentType
            };

            httpRequest.QueryParameters["partNumber"] = _request.PartNumber.ToString(CultureInfo.InvariantCulture);
            httpRequest.QueryParameters["uploadId"] = _request.UploadId;
            return httpRequest;
        }

        public UploadPartResponse ParseResponse(OssHttpResponse response) => new UploadPartResponse
        {
            StatusCode = response.StatusCode,
            ETag = response.Headers.TryGetValue("ETag", out var etag) ? etag : null
        };
    }
}
