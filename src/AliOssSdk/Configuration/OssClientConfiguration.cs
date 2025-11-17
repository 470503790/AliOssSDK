using System;
using System.Collections.Generic;
using AliOssSdk.Http;
using AliOssSdk.Logging;
using AliOssSdk.Security;

namespace AliOssSdk.Configuration
{
    /// <summary>
    /// Represents the configuration required to communicate with Alibaba Cloud OSS.
    /// </summary>
    public sealed class OssClientConfiguration
    {
        private static readonly IEqualityComparer<string> HeaderComparer = StringComparer.OrdinalIgnoreCase;

        public OssClientConfiguration(Uri endpoint, string accessKeyId, string accessKeySecret)
            : this(endpoint, accessKeyId, accessKeySecret, defaultBucketName: null)
        {
        }

        public OssClientConfiguration(Uri endpoint, string accessKeyId, string accessKeySecret, string? defaultBucketName)
        {
            Endpoint = ValidateEndpoint(endpoint);
            AccessKeyId = ValidateRequired(accessKeyId, nameof(accessKeyId));
            AccessKeySecret = ValidateRequired(accessKeySecret, nameof(accessKeySecret));
            DefaultBucketName = ValidateBucketName(defaultBucketName);
            Timeout = TimeSpan.FromSeconds(100);
            DefaultHeaders = new Dictionary<string, string>(HeaderComparer);
            DefaultQueryParameters = new Dictionary<string, string>(HeaderComparer);
        }

        public OssClientConfiguration(string endpoint, string accessKeyId, string accessKeySecret)
            : this(ParseEndpoint(endpoint), accessKeyId, accessKeySecret, defaultBucketName: null)
        {
        }

        public OssClientConfiguration(string endpoint, string accessKeyId, string accessKeySecret, string? defaultBucketName)
            : this(ParseEndpoint(endpoint), accessKeyId, accessKeySecret, defaultBucketName)
        {
        }

        private OssClientConfiguration(OssClientConfiguration source)
        {
            Endpoint = source.Endpoint;
            AccessKeyId = source.AccessKeyId;
            AccessKeySecret = source.AccessKeySecret;
            SecurityToken = source.SecurityToken;
            Timeout = source.Timeout;
            DefaultRegion = source.DefaultRegion;
            Logger = source.Logger;
            HttpClient = source.HttpClient;
            RequestSigner = source.RequestSigner;
            DefaultBucketName = source.DefaultBucketName;
            UseVirtualHostStyle = source.UseVirtualHostStyle;
            DefaultHeaders = new Dictionary<string, string>(source.DefaultHeaders, HeaderComparer);
            DefaultQueryParameters = new Dictionary<string, string>(source.DefaultQueryParameters, HeaderComparer);
        }

        public Uri Endpoint { get; }

        public string AccessKeyId { get; }

        public string AccessKeySecret { get; }

        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(100);

        public string? DefaultRegion { get; set; }

        public string? DefaultBucketName { get; set; }

        public string? SecurityToken { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use virtual-host style addressing.
        /// When true, the bucket name is expected in the hostname (e.g., bucket.oss-region.aliyuncs.com/object),
        /// and resource paths will not include the bucket name.
        /// When false (default), path-style addressing is used (e.g., oss-region.aliyuncs.com/bucket/object).
        /// If not explicitly set, this is auto-detected based on the endpoint hostname.
        /// </summary>
        public bool? UseVirtualHostStyle { get; set; }

        public ILogger? Logger { get; set; }

        public IOssHttpClient? HttpClient { get; set; }

        public IOssRequestSigner? RequestSigner { get; set; }

        public IDictionary<string, string> DefaultHeaders { get; }

        public IDictionary<string, string> DefaultQueryParameters { get; }

        public OssClientConfiguration Clone()
        {
            return new OssClientConfiguration(this);
        }

        public OssClientConfiguration WithTimeout(TimeSpan timeout)
        {
            var clone = Clone();
            clone.Timeout = timeout;
            return clone;
        }

        /// <summary>
        /// Determines whether the endpoint is using virtual-host style addressing.
        /// Virtual-host style means the bucket name is in the hostname (e.g., bucket.oss-region.aliyuncs.com).
        /// Path-style means the bucket name is in the path (e.g., oss-region.aliyuncs.com/bucket).
        /// </summary>
        public bool IsVirtualHostStyle(string? bucketName = null)
        {
            // If explicitly set, use that value
            if (UseVirtualHostStyle.HasValue)
            {
                return UseVirtualHostStyle.Value;
            }

            // Auto-detect based on endpoint hostname
            var host = Endpoint.Host;
            
            // Check if hostname starts with a bucket name followed by .oss-
            // Pattern: {bucket}.oss-{region}.aliyuncs.com
            var bucket = bucketName ?? DefaultBucketName;
            if (!string.IsNullOrEmpty(bucket))
            {
                if (host.StartsWith(bucket + ".oss-", StringComparison.OrdinalIgnoreCase) ||
                    host.StartsWith(bucket + ".", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            // Check if the hostname contains .oss- pattern without bucket prefix
            // This indicates path-style (e.g., oss-cn-hangzhou.aliyuncs.com)
            if (host.StartsWith("oss-", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            // If hostname has multiple parts and contains oss-, assume virtual-host
            // (e.g., something.oss-region.aliyuncs.com)
            if (host.Contains(".oss-"))
            {
                return true;
            }

            // Default to path-style for backward compatibility
            return false;
        }

        private static Uri ValidateEndpoint(Uri endpoint)
        {
            if (endpoint == null)
            {
                throw new ArgumentNullException(nameof(endpoint));
            }

            if (!endpoint.IsAbsoluteUri)
            {
                throw new ArgumentException("Endpoint must be an absolute URI.", nameof(endpoint));
            }

            if (!string.Equals(endpoint.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(endpoint.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Endpoint must use HTTP or HTTPS.", nameof(endpoint));
            }

            return endpoint;
        }

        private static Uri ParseEndpoint(string endpoint)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                throw new ArgumentException("Endpoint cannot be null or whitespace.", nameof(endpoint));
            }

            return ValidateEndpoint(new Uri(endpoint, UriKind.Absolute));
        }

        private static string ValidateRequired(string value, string parameterName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException($"{parameterName} cannot be empty.", parameterName);
            }

            return value;
        }

        private static string? ValidateBucketName(string? bucketName)
        {
            if (bucketName == null)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(bucketName))
            {
                throw new ArgumentException("Bucket name cannot be empty when provided.", nameof(bucketName));
            }

            return bucketName;
        }
    }
}
