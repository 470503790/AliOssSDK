namespace AliOssSdk.Models.Buckets
{
    public sealed class GetBucketAclResponse
    {
        public string? OwnerId { get; init; }

        public string? OwnerDisplayName { get; init; }

        public string? Grant { get; init; }
    }
}
