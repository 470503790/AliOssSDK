using System.Threading;
using System.Threading.Tasks;
using AliOssSdk.Operations;

namespace AliOssSdk
{
    /// <summary>
    /// A high level facade for executing strongly-typed OSS operations.
    /// </summary>
    public interface IOssClient
    {
        TResponse Execute<TResponse>(IOssOperation<TResponse> operation);

        Task<TResponse> ExecuteAsync<TResponse>(IOssOperation<TResponse> operation, CancellationToken cancellationToken = default);
    }
}
