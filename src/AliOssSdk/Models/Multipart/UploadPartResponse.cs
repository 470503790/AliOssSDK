using System.Net;

namespace AliOssSdk.Models.Multipart
{
    public sealed class UploadPartResponse
    {
        public HttpStatusCode StatusCode { get; init; }

        public string? ETag { get; init; }
    }
}
