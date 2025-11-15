namespace AliOssSdk.Models.Multipart
{
    public sealed class InitiateMultipartUploadResponse
    {
        public string? UploadId { get; set; }

        public string? Bucket { get; set; }

        public string? Key { get; set; }
    }
}
