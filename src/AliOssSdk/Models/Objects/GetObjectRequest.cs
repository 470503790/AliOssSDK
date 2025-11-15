using System;

namespace AliOssSdk.Models.Objects
{
    public sealed class GetObjectRequest
    {
        public GetObjectRequest(string bucketName, string objectKey)
        {
            BucketName = bucketName ?? throw new ArgumentNullException(nameof(bucketName));
            ObjectKey = objectKey ?? throw new ArgumentNullException(nameof(objectKey));
        }

        public string BucketName { get; }

        public string ObjectKey { get; }
    }
}
