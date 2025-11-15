using AliOssSdk.Http;

namespace AliOssSdk.Operations
{
    /// <summary>
    /// Defines the contract for an OSS operation. New API calls can be modelled by implementing this interface.
    /// </summary>
    public interface IOssOperation<TResponse>
    {
        string Name { get; }

        OssHttpRequest BuildRequest(OssOperationContext context);

        TResponse ParseResponse(OssHttpResponse response);
    }
}
