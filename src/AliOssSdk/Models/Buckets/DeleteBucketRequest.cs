using System;

namespace AliOssSdk.Models.Buckets
{
    public sealed class DeleteBucketRequest
    {
        public DeleteBucketRequest(string? bucketName)
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
