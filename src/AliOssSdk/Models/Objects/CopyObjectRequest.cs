using System;

namespace AliOssSdk.Models.Objects
{
    public sealed class CopyObjectRequest
    {
        public CopyObjectRequest(string sourceBucket, string sourceKey, string destinationBucket, string destinationKey)
        {
            if (string.IsNullOrWhiteSpace(sourceBucket))
            {
                throw new ArgumentException("Source bucket is required", nameof(sourceBucket));
            }

            if (string.IsNullOrWhiteSpace(sourceKey))
            {
                throw new ArgumentException("Source key is required", nameof(sourceKey));
            }

            if (string.IsNullOrWhiteSpace(destinationBucket))
            {
                throw new ArgumentException("Destination bucket is required", nameof(destinationBucket));
            }

            if (string.IsNullOrWhiteSpace(destinationKey))
            {
                throw new ArgumentException("Destination key is required", nameof(destinationKey));
            }

            SourceBucket = sourceBucket;
            SourceKey = sourceKey;
            DestinationBucket = destinationBucket;
            DestinationKey = destinationKey;
        }

        public string SourceBucket { get; }

        public string SourceKey { get; }

        public string DestinationBucket { get; }

        public string DestinationKey { get; }
    }
}
