using System;

namespace AliOssSdk.Models.Objects
{
    public sealed class HeadObjectRequest
    {
        public HeadObjectRequest(string objectKey)
            : this(null, objectKey)
        {
        }

        public HeadObjectRequest(string? bucketName, string objectKey)
        {
            if (string.IsNullOrWhiteSpace(objectKey))
            {
                throw new ArgumentException("Object key is required", nameof(objectKey));
            }

            BucketName = bucketName;
            ObjectKey = objectKey;
        }

        public string? BucketName { get; }

        public string ObjectKey { get; }
    }
}
