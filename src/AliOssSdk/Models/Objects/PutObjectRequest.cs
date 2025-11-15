using System;
using System.IO;

namespace AliOssSdk.Models.Objects
{
    public sealed class PutObjectRequest
    {
        public PutObjectRequest(string bucketName, string objectKey, Stream content)
        {
            BucketName = bucketName ?? throw new ArgumentNullException(nameof(bucketName));
            ObjectKey = objectKey ?? throw new ArgumentNullException(nameof(objectKey));
            Content = content ?? throw new ArgumentNullException(nameof(content));
        }

        public string BucketName { get; }

        public string ObjectKey { get; }

        public Stream Content { get; }

        public string? ContentType { get; init; }
    }
}
