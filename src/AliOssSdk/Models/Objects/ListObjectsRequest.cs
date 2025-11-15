using System;

namespace AliOssSdk.Models.Objects
{
    public sealed class ListObjectsRequest
    {
        public ListObjectsRequest(string? bucketName)
        {
            if (bucketName != null && string.IsNullOrWhiteSpace(bucketName))
            {
                throw new ArgumentException("Bucket name cannot be empty", nameof(bucketName));
            }

            BucketName = bucketName;
        }

        public string? BucketName { get; }

        public string? Prefix { get; set; }

        public string? Delimiter { get; set; }

        public string? Marker { get; set; }

        public int? MaxKeys { get; set; }
    }
}
