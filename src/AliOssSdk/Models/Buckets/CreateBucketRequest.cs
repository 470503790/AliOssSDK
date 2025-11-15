using System;

namespace AliOssSdk.Models.Buckets
{
    public sealed class CreateBucketRequest
    {
        public CreateBucketRequest(string? bucketName)
        {
            if (bucketName != null && string.IsNullOrWhiteSpace(bucketName))
            {
                throw new ArgumentException("Bucket name cannot be empty", nameof(bucketName));
            }

            BucketName = bucketName;
        }

        public string? BucketName { get; }

        public string? Region { get; set; }
    }
}
