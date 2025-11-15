using System;

namespace AliOssSdk.Models.Multipart
{
    public sealed class ListPartsRequest
    {
        public ListPartsRequest(string bucketName, string objectKey, string uploadId)
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

            BucketName = bucketName;
            ObjectKey = objectKey;
            UploadId = uploadId;
        }

        public string BucketName { get; }

        public string ObjectKey { get; }

        public string UploadId { get; }

        public int? PartNumberMarker { get; set; }

        public int? MaxParts { get; set; }
    }
}
