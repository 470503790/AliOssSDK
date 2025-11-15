using System;

namespace AliOssSdk.Models.Objects
{
    public sealed class ObjectMoveDescriptor
    {
        public ObjectMoveDescriptor(string sourceObjectKey, string destinationObjectKey)
            : this(null, sourceObjectKey, null, destinationObjectKey)
        {
        }

        public ObjectMoveDescriptor(string? sourceBucketName, string sourceObjectKey, string? destinationBucketName, string destinationObjectKey)
        {
            if (string.IsNullOrWhiteSpace(sourceObjectKey))
            {
                throw new ArgumentException("Source object key is required", nameof(sourceObjectKey));
            }

            if (string.IsNullOrWhiteSpace(destinationObjectKey))
            {
                throw new ArgumentException("Destination object key is required", nameof(destinationObjectKey));
            }

            SourceBucketName = NormalizeBucket(sourceBucketName, nameof(sourceBucketName));
            DestinationBucketName = NormalizeBucket(destinationBucketName, nameof(destinationBucketName));
            SourceObjectKey = sourceObjectKey;
            DestinationObjectKey = destinationObjectKey;
        }

        public string? SourceBucketName { get; }

        public string SourceObjectKey { get; }

        public string? DestinationBucketName { get; }

        public string DestinationObjectKey { get; }

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
