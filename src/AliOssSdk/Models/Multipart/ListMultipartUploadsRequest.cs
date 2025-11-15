using System;

namespace AliOssSdk.Models.Multipart
{
    public sealed class ListMultipartUploadsRequest
    {
        public ListMultipartUploadsRequest(string bucketName)
        {
            if (string.IsNullOrWhiteSpace(bucketName))
            {
                throw new ArgumentException("Bucket name is required", nameof(bucketName));
            }

            BucketName = bucketName;
        }

        public string BucketName { get; }

        public string? Prefix { get; set; }

        public string? Delimiter { get; set; }

        public string? KeyMarker { get; set; }

        public string? UploadIdMarker { get; set; }

        public int? MaxUploads { get; set; }
    }
}
