using System;

namespace AliOssSdk.Models.Objects
{
    public sealed class ObjectSummary
    {
        public string Key { get; init; } = string.Empty;

        public string? ETag { get; init; }

        public long Size { get; init; }

        public DateTimeOffset? LastModified { get; init; }
    }
}
