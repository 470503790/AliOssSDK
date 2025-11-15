using System.Net;

namespace AliOssSdk.Models.Objects
{
    public sealed class PutObjectResponse
    {
        public HttpStatusCode StatusCode { get; set; }

        public string? ETag { get; set; }
    }
}
