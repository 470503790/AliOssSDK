using System;
using System.IO;
using System.Net.Http;
using System.Text;
using AliOssSdk.Models.Multipart;
using AliOssSdk.Operations.Multipart;
using Xunit;
using AliOssSdk.Tests.Operations;

namespace AliOssSdk.Tests.Operations.Multipart;

public class CompleteMultipartUploadOperationTests
{
    [Fact]
    public void BuildRequest_SerializesPartsInOrder()
    {
        var request = new CompleteMultipartUploadRequest("bucket", "key", "upload", new[]
        {
            new CompleteMultipartUploadRequest.UploadedPart(2, "etag2"),
            new CompleteMultipartUploadRequest.UploadedPart(1, "etag1")
        });
        var operation = new CompleteMultipartUploadOperation(request);

        var result = operation.BuildRequest(OperationTestHelpers.CreateContext());

        Assert.Equal(HttpMethod.Post, result.Method);
        Assert.Equal("/bucket/key", result.ResourcePath);
        Assert.Equal("upload", result.QueryParameters["uploadId"]);
        Assert.Equal("application/xml", result.ContentType);
        using var reader = new StreamReader(result.Content!, Encoding.UTF8, leaveOpen: true);
        var xml = reader.ReadToEnd();
        Assert.Contains("<Part><PartNumber>1</PartNumber><ETag>etag1</ETag></Part>", xml);
        Assert.Contains("<Part><PartNumber>2</PartNumber><ETag>etag2</ETag></Part>", xml);
        Assert.True(xml.IndexOf("etag1", StringComparison.Ordinal) < xml.IndexOf("etag2", StringComparison.Ordinal));
    }

    [Fact]
    public void ParseResponse_ReadsResult()
    {
        const string payload = """
<CompleteMultipartUploadResult>
  <Location>https://example/bucket/key</Location>
  <Bucket>bucket</Bucket>
  <Key>key</Key>
  <ETag>"etag"</ETag>
</CompleteMultipartUploadResult>
""";
        var response = OperationTestHelpers.CreateResponse(payload);
        var operation = new CompleteMultipartUploadOperation(new CompleteMultipartUploadRequest("bucket", "key", "upload", new[]
        {
            new CompleteMultipartUploadRequest.UploadedPart(1, "etag1")
        }));

        var result = operation.ParseResponse(response);

        Assert.Equal("https://example/bucket/key", result.Location);
        Assert.Equal("bucket", result.Bucket);
        Assert.Equal("key", result.Key);
        Assert.Equal("etag", result.ETag);
    }
}
