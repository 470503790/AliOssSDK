using System.Collections.Generic;

namespace AliOssSdk.Models.Objects
{
    public sealed class ListObjectsResponse
    {
        public IReadOnlyCollection<ObjectSummary> Objects { get; set; } = new List<ObjectSummary>();

        public string? NextMarker { get; set; }

        public bool IsTruncated { get; set; }
    }
}
