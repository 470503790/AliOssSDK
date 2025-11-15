using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AliOssSdk.Configuration;
using AliOssSdk.Http;
using AliOssSdk.Models.Buckets;
using AliOssSdk.Models.Multipart;
using AliOssSdk.Models.Objects;
using AliOssSdk.Operations;
using AliOssSdk.Operations.Buckets;
using AliOssSdk.Operations.Multipart;
using AliOssSdk.Operations.Objects;
using AliOssSdk.Security;
using AliOssSdk.Logging;

namespace AliOssSdk
{
    public sealed class OssClient : IOssClient, IDisposable
    {
        private readonly IOssHttpClient _httpClient;
        private readonly IOssRequestSigner _requestSigner;
        private readonly ILogger _logger;
        private readonly bool _ownsHttpClient;
        private readonly OssOperationContext _context;

        public OssClient(OssClientConfiguration configuration)
            : this(
                configuration ?? throw new ArgumentNullException(nameof(configuration)),
                configuration.HttpClient ?? new OssHttpClient(configuration),
                configuration.RequestSigner ?? new OssRequestSignerV4(),
                configuration.Logger ?? OssLoggerRegistry.Logger,
                ownsHttpClient: configuration.HttpClient == null)
        {
        }

        public OssClient(OssClientConfiguration configuration, IOssHttpClient httpClient, IOssRequestSigner requestSigner, bool ownsHttpClient = false)
            : this(configuration, httpClient, requestSigner, OssLoggerRegistry.Logger, ownsHttpClient)
        {
        }

        public OssClient(OssClientConfiguration configuration, IOssHttpClient httpClient, IOssRequestSigner requestSigner, ILogger logger, bool ownsHttpClient = false)
        {
            _context = new OssOperationContext(configuration ?? throw new ArgumentNullException(nameof(configuration)));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _requestSigner = requestSigner ?? throw new ArgumentNullException(nameof(requestSigner));
            _logger = logger ?? OssLoggerRegistry.Logger;
            _ownsHttpClient = ownsHttpClient;
        }

        public TResponse Execute<TResponse>(IOssOperation<TResponse> operation) => ExecuteAsync(operation).GetAwaiter().GetResult();

        public async Task<TResponse> ExecuteAsync<TResponse>(IOssOperation<TResponse> operation, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (operation == null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            var invocationId = Guid.NewGuid();
            var attempt = 1;
            var operationName = operation.GetType().Name;
            OssHttpRequest? request = null;
            var start = DateTimeOffset.UtcNow;

            LogRetry(operationName, invocationId, attempt, isRetry: attempt > 1, exception: null);

            try
            {
                request = operation.BuildRequest(_context);
                _requestSigner.Sign(request, _context.Configuration);

                LogRequestStart(operationName, invocationId, attempt, request);
                LogRequestHeaders(operationName, invocationId, attempt, request);
                LogRequestBody(operationName, invocationId, attempt, request);

                var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
                LogResponse(operationName, invocationId, attempt, response, DateTimeOffset.UtcNow - start);
                return operation.ParseResponse(response);
            }
            catch (Exception ex)
            {
                LogError(operationName, invocationId, attempt, request, ex);
                throw;
            }
        }

        #region Bucket helpers
        public ListBucketsResponse ListBuckets(ListBucketsRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return Execute(new ListBucketsOperation(request));
        }

        public Task<ListBucketsResponse> ListBucketsAsync(ListBucketsRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return ExecuteAsync(new ListBucketsOperation(request), cancellationToken);
        }

        public CreateBucketResponse CreateBucket(CreateBucketRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return Execute(new CreateBucketOperation(request));
        }

        public Task<CreateBucketResponse> CreateBucketAsync(CreateBucketRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return ExecuteAsync(new CreateBucketOperation(request), cancellationToken);
        }

        public DeleteBucketResponse DeleteBucket(DeleteBucketRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return Execute(new DeleteBucketOperation(request));
        }

        public Task<DeleteBucketResponse> DeleteBucketAsync(DeleteBucketRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return ExecuteAsync(new DeleteBucketOperation(request), cancellationToken);
        }

        public GetBucketInfoResponse GetBucketInfo(GetBucketInfoRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return Execute(new GetBucketInfoOperation(request));
        }

        public Task<GetBucketInfoResponse> GetBucketInfoAsync(GetBucketInfoRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return ExecuteAsync(new GetBucketInfoOperation(request), cancellationToken);
        }

        public GetBucketAclResponse GetBucketAcl(GetBucketAclRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return Execute(new GetBucketAclOperation(request));
        }

        public Task<GetBucketAclResponse> GetBucketAclAsync(GetBucketAclRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return ExecuteAsync(new GetBucketAclOperation(request), cancellationToken);
        }

        public PutBucketAclResponse PutBucketAcl(PutBucketAclRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return Execute(new PutBucketAclOperation(request));
        }

        public Task<PutBucketAclResponse> PutBucketAclAsync(PutBucketAclRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return ExecuteAsync(new PutBucketAclOperation(request), cancellationToken);
        }
        #endregion

        #region Object helpers
        public PutObjectResponse PutObject(PutObjectRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return Execute(new PutObjectOperation(request));
        }

        public Task<PutObjectResponse> PutObjectAsync(PutObjectRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return ExecuteAsync(new PutObjectOperation(request), cancellationToken);
        }

        public PutObjectResponse PutObjectFromFile(string? bucketName, string objectKey, string filePath, string? contentType = null)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("File path is required", nameof(filePath));
            }

            using (var stream = File.OpenRead(filePath))
            {
                var request = new PutObjectRequest(bucketName, objectKey, stream)
                {
                    ContentType = contentType
                };
                return PutObject(request);
            }
        }

        public async Task<PutObjectResponse> PutObjectFromFileAsync(string? bucketName, string objectKey, string filePath, string? contentType = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("File path is required", nameof(filePath));
            }

            using (var stream = File.OpenRead(filePath))
            {
                var request = new PutObjectRequest(bucketName, objectKey, stream)
                {
                    ContentType = contentType
                };
                return await PutObjectAsync(request, cancellationToken).ConfigureAwait(false);
            }
        }

        public GetObjectResponse GetObject(GetObjectRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return Execute(new GetObjectOperation(request));
        }

        public Task<GetObjectResponse> GetObjectAsync(GetObjectRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return ExecuteAsync(new GetObjectOperation(request), cancellationToken);
        }

        public DeleteObjectResponse DeleteObject(DeleteObjectRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return Execute(new DeleteObjectOperation(request));
        }

        public Task<DeleteObjectResponse> DeleteObjectAsync(DeleteObjectRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return ExecuteAsync(new DeleteObjectOperation(request), cancellationToken);
        }

        public ListObjectsResponse ListObjects(ListObjectsRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return Execute(new ListObjectsOperation(request));
        }

        public Task<ListObjectsResponse> ListObjectsAsync(ListObjectsRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return ExecuteAsync(new ListObjectsOperation(request), cancellationToken);
        }

        public HeadObjectResponse HeadObject(HeadObjectRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return Execute(new HeadObjectOperation(request));
        }

        public Task<HeadObjectResponse> HeadObjectAsync(HeadObjectRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return ExecuteAsync(new HeadObjectOperation(request), cancellationToken);
        }

        public CopyObjectResponse CopyObject(CopyObjectRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return Execute(new CopyObjectOperation(request));
        }

        public Task<CopyObjectResponse> CopyObjectAsync(CopyObjectRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return ExecuteAsync(new CopyObjectOperation(request), cancellationToken);
        }

        public void MoveObjects(IEnumerable<ObjectMoveDescriptor> descriptors)
        {
            if (descriptors == null)
            {
                throw new ArgumentNullException(nameof(descriptors));
            }

            foreach (var descriptor in descriptors)
            {
                if (descriptor == null)
                {
                    throw new ArgumentException("Move descriptor cannot be null", nameof(descriptors));
                }

                var copyRequest = new CopyObjectRequest(descriptor.SourceBucketName, descriptor.SourceObjectKey, descriptor.DestinationBucketName, descriptor.DestinationObjectKey);
                CopyObject(copyRequest);

                var deleteBucket = descriptor.SourceBucketName ?? descriptor.DestinationBucketName;
                var deleteRequest = new DeleteObjectRequest(deleteBucket, descriptor.SourceObjectKey);
                DeleteObject(deleteRequest);
            }
        }

        public async Task MoveObjectsAsync(IEnumerable<ObjectMoveDescriptor> descriptors, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (descriptors == null)
            {
                throw new ArgumentNullException(nameof(descriptors));
            }

            foreach (var descriptor in descriptors)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (descriptor == null)
                {
                    throw new ArgumentException("Move descriptor cannot be null", nameof(descriptors));
                }

                var copyRequest = new CopyObjectRequest(descriptor.SourceBucketName, descriptor.SourceObjectKey, descriptor.DestinationBucketName, descriptor.DestinationObjectKey);
                await CopyObjectAsync(copyRequest, cancellationToken).ConfigureAwait(false);

                var deleteBucket = descriptor.SourceBucketName ?? descriptor.DestinationBucketName;
                var deleteRequest = new DeleteObjectRequest(deleteBucket, descriptor.SourceObjectKey);
                await DeleteObjectAsync(deleteRequest, cancellationToken).ConfigureAwait(false);
            }
        }
        #endregion

        #region Multipart helpers
        public InitiateMultipartUploadResponse InitiateMultipartUpload(InitiateMultipartUploadRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return Execute(new InitiateMultipartUploadOperation(request));
        }

        public Task<InitiateMultipartUploadResponse> InitiateMultipartUploadAsync(InitiateMultipartUploadRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return ExecuteAsync(new InitiateMultipartUploadOperation(request), cancellationToken);
        }

        public UploadPartResponse UploadPart(UploadPartRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return Execute(new UploadPartOperation(request));
        }

        public Task<UploadPartResponse> UploadPartAsync(UploadPartRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return ExecuteAsync(new UploadPartOperation(request), cancellationToken);
        }

        public CompleteMultipartUploadResponse CompleteMultipartUpload(CompleteMultipartUploadRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return Execute(new CompleteMultipartUploadOperation(request));
        }

        public Task<CompleteMultipartUploadResponse> CompleteMultipartUploadAsync(CompleteMultipartUploadRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return ExecuteAsync(new CompleteMultipartUploadOperation(request), cancellationToken);
        }

        public AbortMultipartUploadResponse AbortMultipartUpload(AbortMultipartUploadRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return Execute(new AbortMultipartUploadOperation(request));
        }

        public Task<AbortMultipartUploadResponse> AbortMultipartUploadAsync(AbortMultipartUploadRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return ExecuteAsync(new AbortMultipartUploadOperation(request), cancellationToken);
        }

        public ListPartsResponse ListParts(ListPartsRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return Execute(new ListPartsOperation(request));
        }

        public Task<ListPartsResponse> ListPartsAsync(ListPartsRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return ExecuteAsync(new ListPartsOperation(request), cancellationToken);
        }

        public ListMultipartUploadsResponse ListMultipartUploads(ListMultipartUploadsRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return Execute(new ListMultipartUploadsOperation(request));
        }

        public Task<ListMultipartUploadsResponse> ListMultipartUploadsAsync(ListMultipartUploadsRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return ExecuteAsync(new ListMultipartUploadsOperation(request), cancellationToken);
        }
        #endregion

        public void Dispose()
        {
            if (_ownsHttpClient && _httpClient is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        private void LogRequestStart(string operationName, Guid invocationId, int attempt, OssHttpRequest request)
        {
            var data = new Dictionary<string, object?>
            {
                ["Method"] = request.Method.Method,
                ["ResourcePath"] = request.ResourcePath,
                ["Query"] = request.QueryParameters.Count == 0 ? null : string.Join("&", request.QueryParameters.Select(p => $"{p.Key}={p.Value}"))
            };

            _logger.Log(OssLogEvent.RequestStart(operationName, invocationId, attempt, data));
        }

        private void LogRequestHeaders(string operationName, Guid invocationId, int attempt, OssHttpRequest request)
        {
            var headers = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            foreach (var header in request.Headers)
            {
                headers[header.Key] = header.Value;
            }

            if (headers.Count > 0)
            {
                _logger.Log(OssLogEvent.RequestHeaders(operationName, invocationId, attempt, headers));
            }
        }

        private void LogRequestBody(string operationName, Guid invocationId, int attempt, OssHttpRequest request)
        {
            var payload = new Dictionary<string, object?>
            {
                ["HasContent"] = request.Content != null,
                ["ContentType"] = request.ContentType,
                ["ContentLength"] = TryGetContentLength(request.Content)
            };

            _logger.Log(OssLogEvent.RequestBody(operationName, invocationId, attempt, payload));
        }

        private void LogRetry(string operationName, Guid invocationId, int attempt, bool isRetry, Exception? exception)
        {
            _logger.Log(OssLogEvent.Retry(operationName, invocationId, attempt, isRetry, exception));
        }

        private void LogResponse(string operationName, Guid invocationId, int attempt, OssHttpResponse response, TimeSpan duration)
        {
            var data = new Dictionary<string, object?>
            {
                ["StatusCode"] = (int)response.StatusCode,
                ["DurationMs"] = duration.TotalMilliseconds,
                ["RequestId"] = response.RequestId
            };

            foreach (var header in response.Headers)
            {
                data[$"Header:{header.Key}"] = header.Value;
            }

            _logger.Log(OssLogEvent.Response(operationName, invocationId, attempt, data));
        }

        private void LogError(string operationName, Guid invocationId, int attempt, OssHttpRequest? request, Exception exception)
        {
            var data = request == null
                ? null
                : new Dictionary<string, object?>
                {
                    ["Method"] = request.Method.Method,
                    ["ResourcePath"] = request.ResourcePath
                };

            _logger.Log(OssLogEvent.Error(operationName, invocationId, attempt, data, exception));
        }

        private static long? TryGetContentLength(Stream? stream)
        {
            if (stream == null)
            {
                return null;
            }

            if (!stream.CanSeek)
            {
                return null;
            }

            try
            {
                return stream.Length;
            }
            catch (NotSupportedException)
            {
                return null;
            }
        }
    }
}
