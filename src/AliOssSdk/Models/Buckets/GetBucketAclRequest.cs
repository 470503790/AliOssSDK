using System;

namespace AliOssSdk.Models.Buckets
{
    public sealed class GetBucketAclRequest
    {
        public GetBucketAclRequest(string? bucketName)
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
