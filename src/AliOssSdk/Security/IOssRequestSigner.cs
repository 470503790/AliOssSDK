using AliOssSdk.Configuration;
using AliOssSdk.Http;

namespace AliOssSdk.Security
{
    public interface IOssRequestSigner
    {
        void Sign(OssHttpRequest request, OssClientConfiguration configuration);
    }
}
