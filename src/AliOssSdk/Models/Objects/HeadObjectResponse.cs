using System;
using System.Collections.Generic;
using System.Net;

namespace AliOssSdk.Models.Objects
{
    public sealed class HeadObjectResponse
    {
        public HttpStatusCode StatusCode { get; init; }

        public long? ContentLength { get; init; }

        public string? ContentType { get; init; }

        public DateTimeOffset? LastModified { get; init; }

        public string? ETag { get; init; }

        public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }
}
