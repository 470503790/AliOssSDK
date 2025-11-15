using System;

namespace AliOssSdk.Models.Objects
{
    public sealed class CopyObjectResponse
    {
        public string? ETag { get; init; }

        public DateTimeOffset? LastModified { get; init; }
    }
}
