namespace AliOssSdk.Models.Buckets
{
    public sealed class ListBucketsRequest
    {
        public string? Prefix { get; init; }

        public int? MaxKeys { get; init; }
    }
}
