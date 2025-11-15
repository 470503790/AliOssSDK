using System;
using System.IO;

namespace AliOssSdk.Models.Objects
{
    public sealed class PutObjectRequest
    {
        public PutObjectRequest(string? bucketName, string objectKey, Stream content)
        {
            if (string.IsNullOrWhiteSpace(objectKey))
            {
                throw new ArgumentException("Object key is required", nameof(objectKey));
            }

            BucketName = bucketName;
            ObjectKey = objectKey;
            Content = content ?? throw new ArgumentNullException(nameof(content));
        }

        public string? BucketName { get; }

        public string ObjectKey { get; }

        public Stream Content { get; }

        public string? ContentType { get; set; }
    }
}
