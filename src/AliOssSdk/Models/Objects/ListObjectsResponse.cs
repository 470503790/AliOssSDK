using System.Collections.Generic;

namespace AliOssSdk.Models.Objects
{
    public sealed class ListObjectsResponse
    {
        public IReadOnlyCollection<ObjectSummary> Objects { get; init; } = new List<ObjectSummary>();

        public string? NextMarker { get; init; }

        public bool IsTruncated { get; init; }
    }
}
