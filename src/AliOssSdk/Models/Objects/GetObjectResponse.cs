using System.IO;
using System.Net;

namespace AliOssSdk.Models.Objects
{
    public sealed class GetObjectResponse
    {
        public HttpStatusCode StatusCode { get; set; }

        public Stream Content { get; set; } = Stream.Null;

        public string? ContentType { get; set; }
    }
}
