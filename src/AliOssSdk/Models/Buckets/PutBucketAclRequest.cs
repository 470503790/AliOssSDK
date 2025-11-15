using System;

namespace AliOssSdk.Models.Buckets
{
    public sealed class PutBucketAclRequest
    {
        public PutBucketAclRequest(string bucketName, BucketAcl acl)
        {
            if (string.IsNullOrWhiteSpace(bucketName))
            {
                throw new ArgumentException("Bucket name is required", nameof(bucketName));
            }

            BucketName = bucketName;
            Acl = acl;
        }

        public string BucketName { get; }

        public BucketAcl Acl { get; }
    }
}
