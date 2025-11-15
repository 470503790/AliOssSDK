namespace AliOssSdk.Models.Buckets
{
    public sealed class GetBucketAclResponse
    {
        public string? OwnerId { get; set; }

        public string? OwnerDisplayName { get; set; }

        public string? Grant { get; set; }
    }
}
