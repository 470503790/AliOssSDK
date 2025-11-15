using System.Collections.Generic;

namespace AliOssSdk.Models.Buckets
{
    public sealed class ListBucketsResponse
    {
        public IReadOnlyCollection<string> Buckets { get; set; } = new List<string>();

        public string? NextMarker { get; set; }
    }
}
