using System;
using System.Threading;
using System.Threading.Tasks;
using AliOssSdk.Configuration;
using AliOssSdk.Http;
using AliOssSdk.Operations;
using AliOssSdk.Security;

namespace AliOssSdk
{
    public sealed class OssClient : IOssClient, IDisposable
    {
        private readonly IOssHttpClient _httpClient;
        private readonly IOssRequestSigner _requestSigner;
        private readonly bool _ownsHttpClient;
        private readonly OssOperationContext _context;

        public OssClient(OssClientConfiguration configuration)
            : this(configuration, new OssHttpClient(configuration), new HmacSha1RequestSigner(), ownsHttpClient: true)
        {
        }

        public OssClient(OssClientConfiguration configuration, IOssHttpClient httpClient, IOssRequestSigner requestSigner, bool ownsHttpClient = false)
        {
            _context = new OssOperationContext(configuration ?? throw new ArgumentNullException(nameof(configuration)));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _requestSigner = requestSigner ?? throw new ArgumentNullException(nameof(requestSigner));
            _ownsHttpClient = ownsHttpClient;
        }

        public TResponse Execute<TResponse>(IOssOperation<TResponse> operation) => ExecuteAsync(operation).GetAwaiter().GetResult();

        public async Task<TResponse> ExecuteAsync<TResponse>(IOssOperation<TResponse> operation, CancellationToken cancellationToken = default)
        {
            if (operation == null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            var request = operation.BuildRequest(_context);
            _requestSigner.Sign(request, _context.Configuration);
            var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            return operation.ParseResponse(response);
        }

        public void Dispose()
        {
            if (_ownsHttpClient && _httpClient is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
