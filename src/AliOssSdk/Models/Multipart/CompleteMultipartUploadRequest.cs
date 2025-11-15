using System;
using System.Collections.Generic;
using System.Linq;

namespace AliOssSdk.Models.Multipart
{
    public sealed class CompleteMultipartUploadRequest
    {
        public CompleteMultipartUploadRequest(string? bucketName, string objectKey, string uploadId, IEnumerable<UploadedPart> parts)
        {
            if (bucketName != null && string.IsNullOrWhiteSpace(bucketName))
            {
                throw new ArgumentException("Bucket name cannot be empty", nameof(bucketName));
            }

            if (string.IsNullOrWhiteSpace(objectKey))
            {
                throw new ArgumentException("Object key is required", nameof(objectKey));
            }

            if (string.IsNullOrWhiteSpace(uploadId))
            {
                throw new ArgumentException("UploadId is required", nameof(uploadId));
            }

            if (parts == null)
            {
                throw new ArgumentNullException(nameof(parts));
            }

            var materialized = parts.ToList();
            if (materialized.Count == 0)
            {
                throw new ArgumentException("At least one part must be supplied", nameof(parts));
            }

            BucketName = bucketName;
            ObjectKey = objectKey;
            UploadId = uploadId;
            Parts = materialized;
        }

        public string? BucketName { get; }

        public string ObjectKey { get; }

        public string UploadId { get; }

        public IReadOnlyCollection<UploadedPart> Parts { get; }

        public sealed class UploadedPart
        {
            public UploadedPart(int partNumber, string etag)
            {
                if (partNumber < 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(partNumber), "Part numbers start at 1");
                }

                if (string.IsNullOrWhiteSpace(etag))
                {
                    throw new ArgumentException("ETag is required", nameof(etag));
                }

                PartNumber = partNumber;
                ETag = etag;
            }

            public int PartNumber { get; }

            public string ETag { get; }
        }
    }
}
