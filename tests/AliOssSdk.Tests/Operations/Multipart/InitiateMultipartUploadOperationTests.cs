using System.Net.Http;
using AliOssSdk.Models.Multipart;
using AliOssSdk.Operations.Multipart;
using Xunit;
using AliOssSdk.Tests.Operations;

namespace AliOssSdk.Tests.Operations.Multipart
{

    public class InitiateMultipartUploadOperationTests
    {
        [Fact]
        public void BuildRequest_SetsUploadsQueryAndContentType()
        {
            var request = new InitiateMultipartUploadRequest("bucket", "key")
            {
                ContentType = "text/plain"
            };
            var operation = new InitiateMultipartUploadOperation(request);

            var result = operation.BuildRequest(OperationTestHelpers.CreateContext());

            Assert.Equal(HttpMethod.Post, result.Method);
            Assert.Equal("/bucket/key", result.ResourcePath);
            Assert.True(result.QueryParameters.ContainsKey("uploads"));
            Assert.Equal("text/plain", result.Headers["Content-Type"]);
        }

        [Fact]
        public void ParseResponse_ReturnsIdentifiers()
        {
            const string payload = """
    <InitiateMultipartUploadResult>
      <Bucket>bucket</Bucket>
      <Key>key</Key>
      <UploadId>upload</UploadId>
    </InitiateMultipartUploadResult>
    """;
            var response = OperationTestHelpers.CreateResponse(payload);
            var operation = new InitiateMultipartUploadOperation(new InitiateMultipartUploadRequest("bucket", "key"));

            var result = operation.ParseResponse(response);

            Assert.Equal("bucket", result.Bucket);
            Assert.Equal("key", result.Key);
            Assert.Equal("upload", result.UploadId);
        }
    }
}
