using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Xml.Linq;
using AliOssSdk.Http;
using AliOssSdk.Models.Multipart;

namespace AliOssSdk.Operations.Multipart
{
    public sealed class CompleteMultipartUploadOperation : IOssOperation<CompleteMultipartUploadResponse>
    {
        private readonly CompleteMultipartUploadRequest _request;

        public CompleteMultipartUploadOperation(CompleteMultipartUploadRequest request)
        {
            _request = request ?? throw new ArgumentNullException(nameof(request));
        }

        public string Name => "CompleteMultipartUpload";

        public OssHttpRequest BuildRequest(OssOperationContext context)
        {
            var bucket = context.ResolveBucketName(_request.BucketName);
            var resource = $"/{bucket}/{_request.ObjectKey}";
            var document = new XDocument(new XElement("CompleteMultipartUpload",
                _request.Parts.OrderBy(p => p.PartNumber).Select(part =>
                    new XElement("Part",
                        new XElement("PartNumber", part.PartNumber),
                        new XElement("ETag", part.ETag)))))
            {
                Declaration = new XDeclaration("1.0", "utf-8", "yes")
            };

            var payload = Encoding.UTF8.GetBytes(document.ToString(SaveOptions.DisableFormatting));
            var stream = new MemoryStream(payload);

            var httpRequest = new OssHttpRequest(HttpMethod.Post, resource)
            {
                Content = stream,
                ContentType = "application/xml"
            };

            httpRequest.QueryParameters["uploadId"] = _request.UploadId;
            return httpRequest;
        }

        public CompleteMultipartUploadResponse ParseResponse(OssHttpResponse response)
        {
            try
            {
                var document = XDocument.Load(response.ContentStream);
                var root = document.Root;
                return new CompleteMultipartUploadResponse
                {
                    Location = root?.Element("Location")?.Value,
                    Bucket = root?.Element("Bucket")?.Value,
                    Key = root?.Element("Key")?.Value,
                    ETag = root?.Element("ETag")?.Value?.Trim('"')
                };
            }
            catch
            {
                return new CompleteMultipartUploadResponse();
            }
        }
    }
}
