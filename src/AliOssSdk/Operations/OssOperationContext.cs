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
    }
}
