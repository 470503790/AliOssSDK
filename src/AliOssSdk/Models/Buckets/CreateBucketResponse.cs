using System.Net;

namespace AliOssSdk.Models.Buckets
{
    public sealed class CreateBucketResponse
    {
        public HttpStatusCode StatusCode { get; set; }

        public string? Location { get; set; }
    }
}
