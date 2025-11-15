using System.IO;
using System.Net;
using System.Net.Http;
using AliOssSdk.Models.Objects;
using AliOssSdk.Operations.Objects;
using Xunit;
using AliOssSdk.Tests.Operations;

namespace AliOssSdk.Tests.Operations.Objects;

public class PutObjectOperationTests
{
    [Fact]
    public void BuildRequest_AttachesContentAndDefaultsContentType()
    {
        using var stream = new MemoryStream(new byte[] { 1, 2 });
        var request = new PutObjectRequest("bucket", "key", stream);
        var operation = new PutObjectOperation(request);

        var result = operation.BuildRequest(OperationTestHelpers.CreateContext());

        Assert.Equal(HttpMethod.Put, result.Method);
        Assert.Equal("/bucket/key", result.ResourcePath);
        Assert.Same(stream, result.Content);
        Assert.Equal("application/octet-stream", result.ContentType);
    }

    [Fact]
    public void ParseResponse_ReadsStatusAndEtag()
    {
        var headers = OperationTestHelpers.Headers(("ETag", "\"abc\""));
        var response = OperationTestHelpers.CreateResponse(string.Empty, HttpStatusCode.OK, headers);
        var operation = new PutObjectOperation(new PutObjectRequest("bucket", "key", Stream.Null));

        var result = operation.ParseResponse(response);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.Equal("\"abc\"", result.ETag);
    }
}
