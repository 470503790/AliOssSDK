using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;

namespace AliOssSdk.Http
{
    public sealed class OssHttpRequest
    {
        public OssHttpRequest(HttpMethod method, string resourcePath)
        {
            Method = method ?? throw new ArgumentNullException(nameof(method));
            ResourcePath = resourcePath ?? throw new ArgumentNullException(nameof(resourcePath));
        }

        public HttpMethod Method { get; }

        public string ResourcePath { get; }

        public IDictionary<string, string> QueryParameters { get; } = new Dictionary<string, string>(StringComparer.Ordinal);

        public IDictionary<string, string> Headers { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public Stream? Content { get; set; }

        public string? ContentType { get; set; }
    }
}
