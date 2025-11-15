using System;
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
        public OssClientConfiguration(Uri endpoint, string accessKeyId, string accessKeySecret)
        {
            Endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
            AccessKeyId = accessKeyId ?? throw new ArgumentNullException(nameof(accessKeyId));
            AccessKeySecret = accessKeySecret ?? throw new ArgumentNullException(nameof(accessKeySecret));
        }

        public Uri Endpoint { get; }

        public string AccessKeyId { get; }

        public string AccessKeySecret { get; }

        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(100);

        public string? DefaultRegion { get; set; }

        public ILogger? Logger { get; set; }

        public IOssHttpClient? HttpClient { get; set; }

        public IOssRequestSigner? RequestSigner { get; set; }

        public OssClientConfiguration WithTimeout(TimeSpan timeout)
        {
            return new OssClientConfiguration(Endpoint, AccessKeyId, AccessKeySecret)
            {
                Timeout = timeout,
                DefaultRegion = this.DefaultRegion,
                Logger = this.Logger,
                HttpClient = this.HttpClient,
                RequestSigner = this.RequestSigner
            };
        }
    }
}
