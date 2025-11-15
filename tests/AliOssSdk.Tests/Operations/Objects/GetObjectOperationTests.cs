using System.IO;
using System.Net;
using System.Net.Http;
using AliOssSdk.Models.Objects;
using AliOssSdk.Operations.Objects;
using Xunit;
using AliOssSdk.Tests.Operations;

namespace AliOssSdk.Tests.Operations.Objects
{

    public class GetObjectOperationTests
    {
        [Fact]
        public void BuildRequest_TargetsObject()
        {
            var operation = new GetObjectOperation(new GetObjectRequest("bucket", "key"));

            var result = operation.BuildRequest(OperationTestHelpers.CreateContext());

            Assert.Equal(HttpMethod.Get, result.Method);
            Assert.Equal("/bucket/key", result.ResourcePath);
        }

        [Fact]
        public void ParseResponse_ReturnsContentAndMetadata()
        {
            var headers = OperationTestHelpers.Headers(("Content-Type", "text/plain"));
            var stream = new MemoryStream(new byte[] { 1, 2, 3 });
            var response = OperationTestHelpers.CreateResponse(stream, HttpStatusCode.PartialContent, headers);
            var operation = new GetObjectOperation(new GetObjectRequest("bucket", "key"));

            var result = operation.ParseResponse(response);

            Assert.Equal(HttpStatusCode.PartialContent, result.StatusCode);
            Assert.Same(stream, result.Content);
            Assert.Equal("text/plain", result.ContentType);
        }
    }
}
