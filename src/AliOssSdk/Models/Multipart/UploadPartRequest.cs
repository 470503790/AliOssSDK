using System;
using System.IO;

namespace AliOssSdk.Models.Multipart
{
    public sealed class UploadPartRequest
    {
        public UploadPartRequest(string bucketName, string objectKey, string uploadId, int partNumber, Stream content)
        {
            if (string.IsNullOrWhiteSpace(bucketName))
            {
                throw new ArgumentException("Bucket name is required", nameof(bucketName));
            }

            if (string.IsNullOrWhiteSpace(objectKey))
            {
                throw new ArgumentException("Object key is required", nameof(objectKey));
            }

            if (string.IsNullOrWhiteSpace(uploadId))
            {
                throw new ArgumentException("UploadId is required", nameof(uploadId));
            }

            if (partNumber < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(partNumber), "Part numbers start at 1");
            }

            BucketName = bucketName;
            ObjectKey = objectKey;
            UploadId = uploadId;
            PartNumber = partNumber;
            Content = content ?? throw new ArgumentNullException(nameof(content));
        }

        public string BucketName { get; }

        public string ObjectKey { get; }

        public string UploadId { get; }

        public int PartNumber { get; }

        public Stream Content { get; }

        public string? ContentType { get; set; }
    }
}
