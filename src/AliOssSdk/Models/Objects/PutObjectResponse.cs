using System.Net;

namespace AliOssSdk.Models.Objects
{
    public sealed class PutObjectResponse
    {
        public HttpStatusCode StatusCode { get; init; }

        public string? ETag { get; init; }
    }
}
