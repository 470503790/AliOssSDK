using System.Net;
using System.Net.Http;
using AliOssSdk.Models.Multipart;
using AliOssSdk.Operations.Multipart;
using Xunit;
using AliOssSdk.Tests.Operations;

namespace AliOssSdk.Tests.Operations.Multipart;

public class AbortMultipartUploadOperationTests
{
    [Fact]
    public void BuildRequest_SetsUploadId()
    {
        var operation = new AbortMultipartUploadOperation(new AbortMultipartUploadRequest("bucket", "key", "upload"));

        var result = operation.BuildRequest(OperationTestHelpers.CreateContext());

        Assert.Equal(HttpMethod.Delete, result.Method);
        Assert.Equal("/bucket/key", result.ResourcePath);
        Assert.Equal("upload", result.QueryParameters["uploadId"]);
    }

    [Fact]
    public void ParseResponse_ReturnsStatus()
    {
        var response = OperationTestHelpers.CreateResponse(string.Empty, HttpStatusCode.NoContent);
        var operation = new AbortMultipartUploadOperation(new AbortMultipartUploadRequest("bucket", "key", "upload"));

        var result = operation.ParseResponse(response);

        Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
    }
}
