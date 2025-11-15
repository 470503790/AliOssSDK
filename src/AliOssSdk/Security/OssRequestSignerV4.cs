using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using AliOssSdk.Configuration;
using AliOssSdk.Http;

namespace AliOssSdk.Security
{
    public sealed class OssRequestSignerV4 : IOssRequestSigner
    {
        private const string PayloadHashPlaceholder = "UNSIGNED-PAYLOAD";
        private const string AlgorithmName = "OSS4-HMAC-SHA256";
        private const string RequestType = "aliyun_v4_request";
        private const string ServiceName = "oss";
        private readonly Func<DateTimeOffset> _clock;

        public OssRequestSignerV4()
            : this(() => DateTimeOffset.UtcNow)
        {
        }

        public OssRequestSignerV4(Func<DateTimeOffset> clock)
        {
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }

        public void Sign(OssHttpRequest request, OssClientConfiguration configuration)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            var region = ResolveRegion(configuration);
            if (string.IsNullOrWhiteSpace(region))
            {
                throw new InvalidOperationException("Unable to resolve region for signature V4. Set OssClientConfiguration.DefaultRegion or use an OSS endpoint that contains the region (for example oss-cn-hangzhou.aliyuncs.com).");
            }

            var now = _clock().UtcDateTime;
            var requestDate = now.ToString("yyyyMMddTHHmmssZ", CultureInfo.InvariantCulture);
            var requestDateScope = now.ToString("yyyyMMdd", CultureInfo.InvariantCulture);

            var hostHeader = BuildHostHeader(configuration.Endpoint);
            request.Headers["Host"] = hostHeader;
            request.Headers["x-oss-date"] = requestDate;
            request.Headers["x-oss-content-sha256"] = PayloadHashPlaceholder;

            var canonicalRequest = BuildCanonicalRequest(request);
            var canonicalRequestHash = HashHex(canonicalRequest);
            var credentialScope = string.Join("/", new[] { requestDateScope, region, ServiceName, RequestType });
            var stringToSign = string.Join("\n", new[]
            {
                AlgorithmName,
                request.Headers["x-oss-date"],
                credentialScope,
                canonicalRequestHash
            });

            var signingKey = DeriveSigningKey(configuration.AccessKeySecret, requestDateScope, region);
            var signature = ToHex(HmacSha256(signingKey, stringToSign));
            var signedHeaders = GetSignedHeaders(request.Headers);

            request.Headers["Authorization"] = $"{AlgorithmName} Credential={configuration.AccessKeyId}/{credentialScope},SignedHeaders={signedHeaders},Signature={signature}";
        }

        private static string ResolveRegion(OssClientConfiguration configuration)
        {
            var normalized = NormalizeRegion(configuration.DefaultRegion);
            if (!string.IsNullOrEmpty(normalized))
            {
                return normalized;
            }

            return NormalizeRegion(InferRegionFromEndpoint(configuration.Endpoint));
        }

        private static string InferRegionFromEndpoint(Uri endpoint)
        {
            var host = endpoint.Host ?? string.Empty;
            var index = host.IndexOf("oss-", StringComparison.OrdinalIgnoreCase);
            if (index < 0)
            {
                return string.Empty;
            }

            var start = index + 4;
            var end = host.IndexOf('.', start);
            if (end < 0)
            {
                end = host.Length;
            }

            return host.Substring(start, end - start);
        }

        private static string NormalizeRegion(string? region)
        {
            if (string.IsNullOrWhiteSpace(region))
            {
                return string.Empty;
            }

            var trimmed = region.Trim();
            if (trimmed.StartsWith("oss-", StringComparison.OrdinalIgnoreCase))
            {
                trimmed = trimmed.Substring(4);
            }

            return trimmed.ToLowerInvariant();
        }

        private static string BuildCanonicalRequest(OssHttpRequest request)
        {
            var canonicalUri = BuildCanonicalResourcePath(request.ResourcePath);
            var canonicalQuery = BuildCanonicalQueryString(request.QueryParameters);
            var canonicalHeaders = BuildCanonicalHeaders(request.Headers);
            var signedHeaders = GetSignedHeaders(request.Headers);

            return string.Join("\n", new[]
            {
                request.Method.Method,
                canonicalUri,
                canonicalQuery,
                canonicalHeaders,
                signedHeaders,
                PayloadHashPlaceholder
            });
        }

        private static string BuildCanonicalResourcePath(string resourcePath)
        {
            if (string.IsNullOrEmpty(resourcePath) || resourcePath == "/")
            {
                return "/";
            }

            var trimmed = resourcePath.StartsWith("/", StringComparison.Ordinal)
                ? resourcePath.Substring(1)
                : resourcePath;

            var segments = trimmed.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length == 0)
            {
                return "/";
            }

            var builder = new StringBuilder();
            foreach (var segment in segments)
            {
                builder.Append('/');
                builder.Append(PercentEncode(segment));
            }

            if (resourcePath.EndsWith("/", StringComparison.Ordinal))
            {
                builder.Append('/');
            }

            return builder.Length == 0 ? "/" : builder.ToString();
        }

        private static string BuildCanonicalQueryString(IDictionary<string, string> queryParameters)
        {
            if (queryParameters.Count == 0)
            {
                return string.Empty;
            }

            var builder = new StringBuilder();
            var sorted = queryParameters.OrderBy(p => p.Key, StringComparer.Ordinal);
            var first = true;
            foreach (var pair in sorted)
            {
                if (!first)
                {
                    builder.Append('&');
                }

                builder.Append(PercentEncode(pair.Key));
                builder.Append('=');
                builder.Append(PercentEncode(pair.Value ?? string.Empty));
                first = false;
            }

            return builder.ToString();
        }

        private static string BuildCanonicalHeaders(IDictionary<string, string> headers)
        {
            var sorted = new SortedDictionary<string, string>(StringComparer.Ordinal);
            foreach (var header in headers)
            {
                var name = header.Key.Trim().ToLowerInvariant();
                var value = NormalizeWhitespace(header.Value);
                sorted[name] = value;
            }

            var builder = new StringBuilder();
            foreach (var header in sorted)
            {
                builder.Append(header.Key);
                builder.Append(':');
                builder.Append(header.Value);
                builder.Append('\n');
            }

            return builder.ToString();
        }

        private static string GetSignedHeaders(IDictionary<string, string> headers)
        {
            var sorted = headers
                .Select(header => header.Key.Trim().ToLowerInvariant())
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray();

            return string.Join(";", sorted);
        }

        private static string NormalizeWhitespace(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var trimmed = value.Trim();
            var builder = new StringBuilder(trimmed.Length);
            var previousIsWhitespace = false;
            foreach (var ch in trimmed)
            {
                if (char.IsWhiteSpace(ch))
                {
                    if (!previousIsWhitespace)
                    {
                        builder.Append(' ');
                        previousIsWhitespace = true;
                    }
                }
                else
                {
                    builder.Append(ch);
                    previousIsWhitespace = false;
                }
            }

            return builder.ToString();
        }

        private static string PercentEncode(string value)
        {
            var encoded = Uri.EscapeDataString(value ?? string.Empty);
            return encoded
                .Replace("+", "%20")
                .Replace("*", "%2A")
                .Replace("%7E", "~");
        }

        private static string HashHex(string value)
        {
            using var sha256 = SHA256.Create();
            var data = Encoding.UTF8.GetBytes(value);
            var hash = sha256.ComputeHash(data);
            return ToHex(hash);
        }

        private static string ToHex(byte[] data)
        {
            var builder = new StringBuilder(data.Length * 2);
            foreach (var b in data)
            {
                builder.Append(b.ToString("x2", CultureInfo.InvariantCulture));
            }

            return builder.ToString();
        }

        private static byte[] DeriveSigningKey(string secretKey, string dateScope, string region)
        {
            var key = Encoding.UTF8.GetBytes($"aliyun_v4{secretKey}");
            var dateKey = HmacSha256(key, dateScope);
            var regionKey = HmacSha256(dateKey, region);
            var serviceKey = HmacSha256(regionKey, ServiceName);
            return HmacSha256(serviceKey, RequestType);
        }

        private static byte[] HmacSha256(byte[] key, string data)
        {
            using var hmac = new HMACSHA256(key);
            return hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        }

        private static string BuildHostHeader(Uri endpoint)
        {
            if (endpoint.IsDefaultPort)
            {
                return endpoint.Host;
            }

            return $"{endpoint.Host}:{endpoint.Port}";
        }
    }
}
