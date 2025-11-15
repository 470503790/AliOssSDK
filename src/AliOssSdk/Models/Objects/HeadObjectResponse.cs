using System;
using System.Collections.Generic;
using System.Net;

namespace AliOssSdk.Models.Objects
{
    public sealed class HeadObjectResponse
    {
        public HttpStatusCode StatusCode { get; set; }

        public long? ContentLength { get; set; }

        public string? ContentType { get; set; }

        public DateTimeOffset? LastModified { get; set; }

        public string? ETag { get; set; }

        public IReadOnlyDictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }
}
