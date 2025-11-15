using System.Collections.Generic;
using System.IO;
using System.Net;

namespace AliOssSdk.Http
{
    public sealed class OssHttpResponse
    {
        public OssHttpResponse(HttpStatusCode statusCode, Stream contentStream, IReadOnlyDictionary<string, string> headers)
        {
            StatusCode = statusCode;
            ContentStream = contentStream;
            Headers = headers;
        }

        public HttpStatusCode StatusCode { get; }

        public Stream ContentStream { get; }

        public IReadOnlyDictionary<string, string> Headers { get; }

        public string? RequestId { get; init; }
    }
}
