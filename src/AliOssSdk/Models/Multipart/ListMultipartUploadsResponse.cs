using System;
using System.Collections.Generic;

namespace AliOssSdk.Models.Multipart
{
    public sealed class ListMultipartUploadsResponse
    {
        public IReadOnlyCollection<MultipartUploadSummary> Uploads { get; set; } = new List<MultipartUploadSummary>();

        public bool IsTruncated { get; set; }

        public string? NextKeyMarker { get; set; }

        public string? NextUploadIdMarker { get; set; }
    }

    public sealed class MultipartUploadSummary
    {
        public string? Key { get; set; }

        public string? UploadId { get; set; }

        public DateTimeOffset? Initiated { get; set; }
    }
}
