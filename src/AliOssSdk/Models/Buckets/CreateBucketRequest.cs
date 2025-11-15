using System;

namespace AliOssSdk.Models.Buckets
{
    public sealed class CreateBucketRequest
    {
        public CreateBucketRequest(string bucketName)
        {
            BucketName = bucketName ?? throw new ArgumentNullException(nameof(bucketName));
        }

        public string BucketName { get; }

        public string? Region { get; init; }
    }
}
