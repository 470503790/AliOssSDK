using System;
using System.Net.Http;
using AliOssSdk.Models.Multipart;
using AliOssSdk.Operations.Multipart;
using Xunit;
using AliOssSdk.Tests.Operations;

namespace AliOssSdk.Tests.Operations.Multipart;

public class ListMultipartUploadsOperationTests
{
    [Fact]
    public void BuildRequest_AppliesFilters()
    {
        var request = new ListMultipartUploadsRequest("bucket")
        {
            Prefix = "logs/",
            Delimiter = "/",
            KeyMarker = "key",
            UploadIdMarker = "upload",
            MaxUploads = 5
        };
        var operation = new ListMultipartUploadsOperation(request);

        var result = operation.BuildRequest(OperationTestHelpers.CreateContext());

        Assert.Equal(HttpMethod.Get, result.Method);
        Assert.Equal("/bucket", result.ResourcePath);
        Assert.True(result.QueryParameters.ContainsKey("uploads"));
        Assert.Equal("logs/", result.QueryParameters["prefix"]);
        Assert.Equal("/", result.QueryParameters["delimiter"]);
        Assert.Equal("key", result.QueryParameters["key-marker"]);
        Assert.Equal("upload", result.QueryParameters["upload-id-marker"]);
        Assert.Equal("5", result.QueryParameters["max-uploads"]);
    }

    [Fact]
    public void ParseResponse_ReturnsUploads()
    {
        const string payload = """
<ListMultipartUploadsResult xmlns="http://oss.aliyuncs.com/doc/2012-03-01/">
  <Upload>
    <Key>key1</Key>
    <UploadId>upload1</UploadId>
    <Initiated>2023-08-08T08:08:08Z</Initiated>
  </Upload>
  <IsTruncated>true</IsTruncated>
  <NextKeyMarker>next-key</NextKeyMarker>
  <NextUploadIdMarker>next-upload</NextUploadIdMarker>
</ListMultipartUploadsResult>
""";
        var response = OperationTestHelpers.CreateResponse(payload);
        var operation = new ListMultipartUploadsOperation(new ListMultipartUploadsRequest("bucket"));

        var result = operation.ParseResponse(response);

        Assert.True(result.IsTruncated);
        Assert.Equal("next-key", result.NextKeyMarker);
        Assert.Equal("next-upload", result.NextUploadIdMarker);
        var upload = Assert.Single(result.Uploads);
        Assert.Equal("key1", upload.Key);
        Assert.Equal("upload1", upload.UploadId);
        Assert.Equal(new DateTimeOffset(2023, 8, 8, 8, 8, 8, TimeSpan.Zero), upload.Initiated);
    }
}
