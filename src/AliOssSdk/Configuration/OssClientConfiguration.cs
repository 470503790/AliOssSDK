using System;

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

        public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(100);

        public string? DefaultRegion { get; init; }

        public OssClientConfiguration WithTimeout(TimeSpan timeout) => new(Endpoint, AccessKeyId, AccessKeySecret)
        {
            Timeout = timeout,
            DefaultRegion = DefaultRegion
        };
    }
}
