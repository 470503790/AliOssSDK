using System.Net;

namespace AliOssSdk.Models.Multipart
{
    public sealed class UploadPartResponse
    {
        public HttpStatusCode StatusCode { get; set; }

        public string? ETag { get; set; }
    }
}
