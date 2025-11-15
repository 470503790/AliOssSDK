using System;
using System.Collections.Generic;

namespace AliOssSdk.Models.Multipart
{
    public sealed class ListPartsResponse
    {
        public IReadOnlyCollection<MultipartPartSummary> Parts { get; set; } = new List<MultipartPartSummary>();

        public bool IsTruncated { get; set; }

        public int? NextPartNumberMarker { get; set; }
    }

    public sealed class MultipartPartSummary
    {
        public int PartNumber { get; set; }

        public string? ETag { get; set; }

        public long Size { get; set; }

        public DateTimeOffset? LastModified { get; set; }
    }
}
