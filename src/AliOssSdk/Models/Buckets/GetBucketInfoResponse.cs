using System;

namespace AliOssSdk.Models.Buckets
{
    public sealed class GetBucketInfoResponse
    {
        public string? Name { get; init; }

        public string? Location { get; init; }

        public DateTimeOffset? CreationDate { get; init; }
    }
}
