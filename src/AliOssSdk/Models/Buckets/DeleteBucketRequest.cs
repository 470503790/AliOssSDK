using System;

namespace AliOssSdk.Models.Buckets
{
    public sealed class DeleteBucketRequest
    {
        public DeleteBucketRequest(string bucketName)
        {
            if (string.IsNullOrWhiteSpace(bucketName))
            {
                throw new ArgumentException("Bucket name is required", nameof(bucketName));
            }

            BucketName = bucketName;
        }

        public string BucketName { get; }
    }
}
