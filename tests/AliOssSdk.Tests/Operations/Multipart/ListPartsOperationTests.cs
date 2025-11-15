using System;
using System.Net.Http;
using AliOssSdk.Models.Multipart;
using AliOssSdk.Operations.Multipart;
using Xunit;
using AliOssSdk.Tests.Operations;

namespace AliOssSdk.Tests.Operations.Multipart;

public class ListPartsOperationTests
{
    [Fact]
    public void BuildRequest_SetsQueryParameters()
    {
        var request = new ListPartsRequest("bucket", "key", "upload")
        {
            PartNumberMarker = 2,
            MaxParts = 100
        };
        var operation = new ListPartsOperation(request);

        var result = operation.BuildRequest(OperationTestHelpers.CreateContext());

        Assert.Equal(HttpMethod.Get, result.Method);
        Assert.Equal("/bucket/key", result.ResourcePath);
        Assert.Equal("upload", result.QueryParameters["uploadId"]);
        Assert.Equal("2", result.QueryParameters["part-number-marker"]);
        Assert.Equal("100", result.QueryParameters["max-parts"]);
    }

    [Fact]
    public void ParseResponse_ReturnsParts()
    {
        const string payload = """
<ListPartsResult xmlns="http://oss.aliyuncs.com/doc/2012-03-01/">
  <Part>
    <PartNumber>1</PartNumber>
    <ETag>"etag1"</ETag>
    <Size>5</Size>
    <LastModified>2023-09-01T00:00:00Z</LastModified>
  </Part>
  <IsTruncated>true</IsTruncated>
  <NextPartNumberMarker>2</NextPartNumberMarker>
</ListPartsResult>
""";
        var response = OperationTestHelpers.CreateResponse(payload);
        var operation = new ListPartsOperation(new ListPartsRequest("bucket", "key", "upload"));

        var result = operation.ParseResponse(response);

        Assert.True(result.IsTruncated);
        Assert.Equal(2, result.NextPartNumberMarker);
        var part = Assert.Single(result.Parts);
        Assert.Equal(1, part.PartNumber);
        Assert.Equal("etag1", part.ETag);
        Assert.Equal(5, part.Size);
        Assert.Equal(new DateTimeOffset(2023, 9, 1, 0, 0, 0, TimeSpan.Zero), part.LastModified);
    }
}
