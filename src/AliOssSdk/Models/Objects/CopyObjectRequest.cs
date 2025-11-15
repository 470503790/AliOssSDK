using System;

namespace AliOssSdk.Models.Objects
{
    public sealed class CopyObjectRequest
    {
        public CopyObjectRequest(string sourceKey, string destinationKey)
            : this(null, sourceKey, null, destinationKey)
        {
        }

        public CopyObjectRequest(string sourceBucket, string sourceKey, string destinationKey)
            : this(sourceBucket, sourceKey, null, destinationKey)
        {
        }

        public CopyObjectRequest(string? sourceBucket, string sourceKey, string? destinationBucket, string destinationKey)
        {
            if (string.IsNullOrWhiteSpace(sourceKey))
            {
                throw new ArgumentException("Source key is required", nameof(sourceKey));
            }

            if (string.IsNullOrWhiteSpace(destinationKey))
            {
                throw new ArgumentException("Destination key is required", nameof(destinationKey));
            }

            SourceBucket = NormalizeBucket(sourceBucket, nameof(sourceBucket));
            DestinationBucket = NormalizeBucket(destinationBucket, nameof(destinationBucket));

            if (SourceBucket is null && DestinationBucket is not null)
            {
                SourceBucket = DestinationBucket;
            }

            if (DestinationBucket is null && SourceBucket is not null)
            {
                DestinationBucket = SourceBucket;
            }

            SourceKey = sourceKey;
            DestinationKey = destinationKey;
        }

        public string? SourceBucket { get; private set; }

        public string SourceKey { get; }

        public string? DestinationBucket { get; private set; }

        public string DestinationKey { get; }

        private static string? NormalizeBucket(string? bucketName, string argumentName)
        {
            if (bucketName == null)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(bucketName))
            {
                throw new ArgumentException("Bucket name cannot be empty", argumentName);
            }

            return bucketName;
        }
    }
}
