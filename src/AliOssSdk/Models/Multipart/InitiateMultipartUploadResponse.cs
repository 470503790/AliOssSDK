namespace AliOssSdk.Models.Multipart
{
    public sealed class InitiateMultipartUploadResponse
    {
        public string? UploadId { get; init; }

        public string? Bucket { get; init; }

        public string? Key { get; init; }
    }
}
