using System;

namespace AliOssSdk.Models.Objects
{
    public sealed class CopyObjectResponse
    {
        public string? ETag { get; set; }

        public DateTimeOffset? LastModified { get; set; }
    }
}
