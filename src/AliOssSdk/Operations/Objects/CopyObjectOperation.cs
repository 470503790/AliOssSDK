using System;
using System.Globalization;
using System.Net.Http;
using System.Xml.Linq;
using AliOssSdk.Http;
using AliOssSdk.Models.Objects;

namespace AliOssSdk.Operations.Objects
{
    public sealed class CopyObjectOperation : IOssOperation<CopyObjectResponse>
    {
        private readonly CopyObjectRequest _request;

        public CopyObjectOperation(CopyObjectRequest request)
        {
            _request = request ?? throw new ArgumentNullException(nameof(request));
        }

        public string Name => "CopyObject";

        public OssHttpRequest BuildRequest(OssOperationContext context)
        {
            var destinationBucket = context.ResolveBucketName(_request.DestinationBucket ?? _request.SourceBucket);
            var sourceBucket = context.ResolveBucketName(_request.SourceBucket ?? _request.DestinationBucket);

            var resource = context.BuildResourcePath(destinationBucket, _request.DestinationKey);
            var httpRequest = new OssHttpRequest(HttpMethod.Put, resource);
            var source = context.BuildResourcePath(sourceBucket, _request.SourceKey);
            httpRequest.Headers["x-oss-copy-source"] = source;
            return httpRequest;
        }

        public CopyObjectResponse ParseResponse(OssHttpResponse response)
        {
            try
            {
                var document = XDocument.Load(response.ContentStream);
                var result = document.Root;
                DateTimeOffset? lastModified = null;
                var raw = result?.Element("LastModified")?.Value;
                if (!string.IsNullOrWhiteSpace(raw) && DateTimeOffset.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsed))
                {
                    lastModified = parsed;
                }

                return new CopyObjectResponse
                {
                    ETag = result?.Element("ETag")?.Value?.Trim('"'),
                    LastModified = lastModified
                };
            }
            catch
            {
                return new CopyObjectResponse();
            }
        }
    }
}
