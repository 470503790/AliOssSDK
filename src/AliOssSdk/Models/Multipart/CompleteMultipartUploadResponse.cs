namespace AliOssSdk.Models.Multipart
{
    public sealed class CompleteMultipartUploadResponse
    {
        public string? Location { get; set; }

        public string? Bucket { get; set; }

        public string? Key { get; set; }

        public string? ETag { get; set; }
    }
}
