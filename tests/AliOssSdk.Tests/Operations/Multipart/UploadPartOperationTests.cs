using System.IO;
using System.Net;
using System.Net.Http;
using AliOssSdk.Models.Multipart;
using AliOssSdk.Operations.Multipart;
using Xunit;
using AliOssSdk.Tests.Operations;

namespace AliOssSdk.Tests.Operations.Multipart
{

    public class UploadPartOperationTests
    {
        [Fact]
        public void BuildRequest_SetsQueryParameters()
        {
            using (var stream = new MemoryStream(new byte[] { 1 }))
            {
                var request = new UploadPartRequest("bucket", "key", "upload", 3, stream)
                {
                    ContentType = "application/octet-stream"
                };
                var operation = new UploadPartOperation(request);

                var result = operation.BuildRequest(OperationTestHelpers.CreateContext());

                Assert.Equal(HttpMethod.Put, result.Method);
                Assert.Equal("/bucket/key", result.ResourcePath);
                Assert.Equal("3", result.QueryParameters["partNumber"]);
                Assert.Equal("upload", result.QueryParameters["uploadId"]);
                Assert.Same(stream, result.Content);
                Assert.Equal("application/octet-stream", result.ContentType);
            }
        }

        [Fact]
        public void ParseResponse_ReturnsEtag()
        {
            var headers = OperationTestHelpers.Headers(("ETag", "etag"));
            var response = OperationTestHelpers.CreateResponse(string.Empty, HttpStatusCode.Created, headers);
            var operation = new UploadPartOperation(new UploadPartRequest("bucket", "key", "upload", 1, Stream.Null));

            var result = operation.ParseResponse(response);

            Assert.Equal(HttpStatusCode.Created, result.StatusCode);
            Assert.Equal("etag", result.ETag);
        }
    }
}
