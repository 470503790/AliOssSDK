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
