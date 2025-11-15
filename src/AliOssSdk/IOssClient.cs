using System.Threading;
using System.Threading.Tasks;
using AliOssSdk.Models.Buckets;
using AliOssSdk.Models.Multipart;
using AliOssSdk.Models.Objects;
using AliOssSdk.Operations;

namespace AliOssSdk
{
    /// <summary>
    /// A high level facade for executing strongly-typed OSS operations.
    /// </summary>
    public interface IOssClient
    {
        TResponse Execute<TResponse>(IOssOperation<TResponse> operation);

        Task<TResponse> ExecuteAsync<TResponse>(IOssOperation<TResponse> operation, CancellationToken cancellationToken = default(CancellationToken));

        #region Bucket helpers
        ListBucketsResponse ListBuckets(ListBucketsRequest request);

        Task<ListBucketsResponse> ListBucketsAsync(ListBucketsRequest request, CancellationToken cancellationToken = default(CancellationToken));

        CreateBucketResponse CreateBucket(CreateBucketRequest request);

        Task<CreateBucketResponse> CreateBucketAsync(CreateBucketRequest request, CancellationToken cancellationToken = default(CancellationToken));

        DeleteBucketResponse DeleteBucket(DeleteBucketRequest request);

        Task<DeleteBucketResponse> DeleteBucketAsync(DeleteBucketRequest request, CancellationToken cancellationToken = default(CancellationToken));

        GetBucketInfoResponse GetBucketInfo(GetBucketInfoRequest request);

        Task<GetBucketInfoResponse> GetBucketInfoAsync(GetBucketInfoRequest request, CancellationToken cancellationToken = default(CancellationToken));

        GetBucketAclResponse GetBucketAcl(GetBucketAclRequest request);

        Task<GetBucketAclResponse> GetBucketAclAsync(GetBucketAclRequest request, CancellationToken cancellationToken = default(CancellationToken));

        PutBucketAclResponse PutBucketAcl(PutBucketAclRequest request);

        Task<PutBucketAclResponse> PutBucketAclAsync(PutBucketAclRequest request, CancellationToken cancellationToken = default(CancellationToken));
        #endregion

        #region Object helpers
        PutObjectResponse PutObject(PutObjectRequest request);

        Task<PutObjectResponse> PutObjectAsync(PutObjectRequest request, CancellationToken cancellationToken = default(CancellationToken));

        GetObjectResponse GetObject(GetObjectRequest request);

        Task<GetObjectResponse> GetObjectAsync(GetObjectRequest request, CancellationToken cancellationToken = default(CancellationToken));

        DeleteObjectResponse DeleteObject(DeleteObjectRequest request);

        Task<DeleteObjectResponse> DeleteObjectAsync(DeleteObjectRequest request, CancellationToken cancellationToken = default(CancellationToken));

        ListObjectsResponse ListObjects(ListObjectsRequest request);

        Task<ListObjectsResponse> ListObjectsAsync(ListObjectsRequest request, CancellationToken cancellationToken = default(CancellationToken));

        HeadObjectResponse HeadObject(HeadObjectRequest request);

        Task<HeadObjectResponse> HeadObjectAsync(HeadObjectRequest request, CancellationToken cancellationToken = default(CancellationToken));

        CopyObjectResponse CopyObject(CopyObjectRequest request);

        Task<CopyObjectResponse> CopyObjectAsync(CopyObjectRequest request, CancellationToken cancellationToken = default(CancellationToken));
        #endregion

        #region Multipart helpers
        InitiateMultipartUploadResponse InitiateMultipartUpload(InitiateMultipartUploadRequest request);

        Task<InitiateMultipartUploadResponse> InitiateMultipartUploadAsync(InitiateMultipartUploadRequest request, CancellationToken cancellationToken = default(CancellationToken));

        UploadPartResponse UploadPart(UploadPartRequest request);

        Task<UploadPartResponse> UploadPartAsync(UploadPartRequest request, CancellationToken cancellationToken = default(CancellationToken));

        CompleteMultipartUploadResponse CompleteMultipartUpload(CompleteMultipartUploadRequest request);

        Task<CompleteMultipartUploadResponse> CompleteMultipartUploadAsync(CompleteMultipartUploadRequest request, CancellationToken cancellationToken = default(CancellationToken));

        AbortMultipartUploadResponse AbortMultipartUpload(AbortMultipartUploadRequest request);

        Task<AbortMultipartUploadResponse> AbortMultipartUploadAsync(AbortMultipartUploadRequest request, CancellationToken cancellationToken = default(CancellationToken));

        ListPartsResponse ListParts(ListPartsRequest request);

        Task<ListPartsResponse> ListPartsAsync(ListPartsRequest request, CancellationToken cancellationToken = default(CancellationToken));

        ListMultipartUploadsResponse ListMultipartUploads(ListMultipartUploadsRequest request);

        Task<ListMultipartUploadsResponse> ListMultipartUploadsAsync(ListMultipartUploadsRequest request, CancellationToken cancellationToken = default(CancellationToken));
        #endregion
    }
}
