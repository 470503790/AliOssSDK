using System.Net;

namespace AliOssSdk.Models.Buckets
{
    public sealed class CreateBucketResponse
    {
        public HttpStatusCode StatusCode { get; init; }

        public string? Location { get; init; }
    }
}
