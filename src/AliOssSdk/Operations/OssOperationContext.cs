using System;
using AliOssSdk.Configuration;

namespace AliOssSdk.Operations
{
    /// <summary>
    /// Provides contextual information for building requests.
    /// </summary>
    public sealed class OssOperationContext
    {
        public OssOperationContext(OssClientConfiguration configuration)
        {
            Configuration = configuration;
        }

        public OssClientConfiguration Configuration { get; }

        public string ResolveBucketName(string? bucketName)
        {
            if (!string.IsNullOrWhiteSpace(bucketName))
            {
                return bucketName;
            }

            if (!string.IsNullOrWhiteSpace(Configuration.DefaultBucketName))
            {
                return Configuration.DefaultBucketName!;
            }

            throw new InvalidOperationException(
                "Bucket name is required. Provide it on the request or set OssClientConfiguration.DefaultBucketName.");
        }
    }
}
