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

        /// <summary>
        /// Builds the resource path for a request, taking into account whether virtual-host style is being used.
        /// For virtual-host style (bucket in hostname), returns /objectKey.
        /// For path-style, returns /bucket/objectKey.
        /// </summary>
        public string BuildResourcePath(string bucket, string objectKey)
        {
            if (Configuration.IsVirtualHostStyle(bucket))
            {
                // Virtual-host style: bucket is in hostname, so path is just /objectKey
                return string.IsNullOrEmpty(objectKey) || objectKey == "/" 
                    ? "/" 
                    : (objectKey.StartsWith("/") ? objectKey : "/" + objectKey);
            }
            else
            {
                // Path-style: include bucket in path
                return $"/{bucket}/{objectKey}";
            }
        }

        /// <summary>
        /// Builds the resource path for bucket-level operations.
        /// For virtual-host style, returns /.
        /// For path-style, returns /bucket or /bucket/.
        /// </summary>
        public string BuildBucketResourcePath(string bucket, bool includeTrailingSlash = false)
        {
            if (Configuration.IsVirtualHostStyle(bucket))
            {
                // Virtual-host style: bucket is in hostname, so path is just /
                return "/";
            }
            else
            {
                // Path-style: include bucket in path
                return includeTrailingSlash ? $"/{bucket}/" : $"/{bucket}";
            }
        }
    }
}
