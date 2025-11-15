using System;

namespace AliOssSdk.Models.Multipart
{
    public sealed class InitiateMultipartUploadRequest
    {
        public InitiateMultipartUploadRequest(string? bucketName, string objectKey)
        {
            if (bucketName != null && string.IsNullOrWhiteSpace(bucketName))
            {
                throw new ArgumentException("Bucket name cannot be empty", nameof(bucketName));
            }

            if (string.IsNullOrWhiteSpace(objectKey))
            {
                throw new ArgumentException("Object key is required", nameof(objectKey));
            }

            BucketName = bucketName;
            ObjectKey = objectKey;
        }

        public string? BucketName { get; }

        public string ObjectKey { get; }

        public string? ContentType { get; set; }
    }
}
