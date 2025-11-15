using System;

namespace AliOssSdk.Models.Buckets
{
    public sealed class GetBucketInfoResponse
    {
        public string? Name { get; set; }

        public string? Location { get; set; }

        public DateTimeOffset? CreationDate { get; set; }
    }
}
