using System.IO;
using System.Net;

namespace AliOssSdk.Models.Objects
{
    public sealed class GetObjectResponse
    {
        public HttpStatusCode StatusCode { get; init; }

        public Stream Content { get; init; } = Stream.Null;

        public string? ContentType { get; init; }
    }
}
