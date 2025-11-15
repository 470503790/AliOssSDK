using System;

namespace AliOssSdk.Models.Objects
{
    public sealed class HeadObjectRequest
    {
        public HeadObjectRequest(string bucketName, string objectKey)
        {
            if (string.IsNullOrWhiteSpace(bucketName))
            {
                throw new ArgumentException("Bucket name is required", nameof(bucketName));
            }

            if (string.IsNullOrWhiteSpace(objectKey))
            {
                throw new ArgumentException("Object key is required", nameof(objectKey));
            }

            BucketName = bucketName;
            ObjectKey = objectKey;
        }

        public string BucketName { get; }

        public string ObjectKey { get; }
    }
}
