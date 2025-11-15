using System;
using System.Net.Http;
using System.Xml.Linq;
using AliOssSdk.Http;
using AliOssSdk.Models.Multipart;

namespace AliOssSdk.Operations.Multipart
{
    public sealed class InitiateMultipartUploadOperation : IOssOperation<InitiateMultipartUploadResponse>
    {
        private readonly InitiateMultipartUploadRequest _request;

        public InitiateMultipartUploadOperation(InitiateMultipartUploadRequest request)
        {
            _request = request ?? throw new ArgumentNullException(nameof(request));
        }

        public string Name => "InitiateMultipartUpload";

        public OssHttpRequest BuildRequest(OssOperationContext context)
        {
            var resource = $"/{_request.BucketName}/{_request.ObjectKey}";
            var httpRequest = new OssHttpRequest(HttpMethod.Post, resource);
            httpRequest.QueryParameters["uploads"] = string.Empty;
            if (!string.IsNullOrWhiteSpace(_request.ContentType))
            {
                httpRequest.Headers["Content-Type"] = _request.ContentType!;
            }

            return httpRequest;
        }

        public InitiateMultipartUploadResponse ParseResponse(OssHttpResponse response)
        {
            try
            {
                var document = XDocument.Load(response.ContentStream);
                var root = document.Root;
                return new InitiateMultipartUploadResponse
                {
                    Bucket = root?.Element("Bucket")?.Value,
                    Key = root?.Element("Key")?.Value,
                    UploadId = root?.Element("UploadId")?.Value
                };
            }
            catch
            {
                return new InitiateMultipartUploadResponse();
            }
        }
    }
}
