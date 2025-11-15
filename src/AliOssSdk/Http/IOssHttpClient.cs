using System.Threading;
using System.Threading.Tasks;

namespace AliOssSdk.Http
{
    public interface IOssHttpClient
    {
        OssHttpResponse Send(OssHttpRequest request);

        Task<OssHttpResponse> SendAsync(OssHttpRequest request, CancellationToken cancellationToken = default);
    }
}
