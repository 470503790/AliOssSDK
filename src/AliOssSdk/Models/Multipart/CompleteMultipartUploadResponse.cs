namespace AliOssSdk.Models.Multipart
{
    public sealed class CompleteMultipartUploadResponse
    {
        public string? Location { get; init; }

        public string? Bucket { get; init; }

        public string? Key { get; init; }

        public string? ETag { get; init; }
    }
}
