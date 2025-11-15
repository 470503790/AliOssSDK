using System;

namespace AliOssSdk.Models.Objects
{
    public sealed class ObjectSummary
    {
        public string Key { get; set; } = string.Empty;

        public string? ETag { get; set; }

        public long Size { get; set; }

        public DateTimeOffset? LastModified { get; set; }
    }
}
