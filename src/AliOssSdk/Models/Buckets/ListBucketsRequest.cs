namespace AliOssSdk.Models.Buckets
{
    public sealed class ListBucketsRequest
    {
        public string? Prefix { get; set; }

        public int? MaxKeys { get; set; }
    }
}
