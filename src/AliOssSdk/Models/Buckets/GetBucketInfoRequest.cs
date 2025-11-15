using System;

namespace AliOssSdk.Models.Buckets
{
    public sealed class GetBucketInfoRequest
    {
        public GetBucketInfoRequest(string? bucketName)
        {
            if (bucketName != null && string.IsNullOrWhiteSpace(bucketName))
            {
                throw new ArgumentException("Bucket name cannot be empty", nameof(bucketName));
            }

            BucketName = bucketName;
        }

        public string? BucketName { get; }
    }
}
