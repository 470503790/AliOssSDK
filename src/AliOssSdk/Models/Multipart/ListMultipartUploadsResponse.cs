using System;
using System.Collections.Generic;

namespace AliOssSdk.Models.Multipart
{
    public sealed class ListMultipartUploadsResponse
    {
        public IReadOnlyCollection<MultipartUploadSummary> Uploads { get; init; } = new List<MultipartUploadSummary>();

        public bool IsTruncated { get; init; }

        public string? NextKeyMarker { get; init; }

        public string? NextUploadIdMarker { get; init; }
    }

    public sealed class MultipartUploadSummary
    {
        public string? Key { get; init; }

        public string? UploadId { get; init; }

        public DateTimeOffset? Initiated { get; init; }
    }
}
