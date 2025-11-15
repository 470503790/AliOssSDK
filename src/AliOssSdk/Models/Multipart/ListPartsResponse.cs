using System;
using System.Collections.Generic;

namespace AliOssSdk.Models.Multipart
{
    public sealed class ListPartsResponse
    {
        public IReadOnlyCollection<MultipartPartSummary> Parts { get; init; } = new List<MultipartPartSummary>();

        public bool IsTruncated { get; init; }

        public int? NextPartNumberMarker { get; init; }
    }

    public sealed class MultipartPartSummary
    {
        public int PartNumber { get; init; }

        public string? ETag { get; init; }

        public long Size { get; init; }

        public DateTimeOffset? LastModified { get; init; }
    }
}
