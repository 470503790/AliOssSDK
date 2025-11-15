using System;
using System.Net.Http;
using AliOssSdk.Models.Objects;
using AliOssSdk.Operations.Objects;
using Xunit;
using AliOssSdk.Tests.Operations;

namespace AliOssSdk.Tests.Operations.Objects;

public class CopyObjectOperationTests
{
    [Fact]
    public void BuildRequest_SetsSourceHeader()
    {
        var request = new CopyObjectRequest("src", "file.txt", "dest", "copy.txt");
        var operation = new CopyObjectOperation(request);

        var result = operation.BuildRequest(OperationTestHelpers.CreateContext());

        Assert.Equal(HttpMethod.Put, result.Method);
        Assert.Equal("/dest/copy.txt", result.ResourcePath);
        Assert.Equal("/src/file.txt", result.Headers["x-oss-copy-source"]);
    }

    [Fact]
    public void ParseResponse_ReadsEtagAndLastModified()
    {
        const string payload = """
<CopyObjectResult>
  <ETag>"etag"</ETag>
  <LastModified>2023-10-10T01:02:03Z</LastModified>
</CopyObjectResult>
""";
        var response = OperationTestHelpers.CreateResponse(payload);
        var operation = new CopyObjectOperation(new CopyObjectRequest("src", "file", "dest", "copy"));

        var result = operation.ParseResponse(response);

        Assert.Equal("etag", result.ETag);
        Assert.Equal(new DateTimeOffset(2023, 10, 10, 1, 2, 3, TimeSpan.Zero), result.LastModified);
    }
}
