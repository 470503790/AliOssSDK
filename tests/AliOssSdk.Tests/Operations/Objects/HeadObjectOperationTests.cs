using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using AliOssSdk.Models.Objects;
using AliOssSdk.Operations.Objects;
using Xunit;
using AliOssSdk.Tests.Operations;

namespace AliOssSdk.Tests.Operations.Objects;

public class HeadObjectOperationTests
{
    [Fact]
    public void BuildRequest_UsesHeadVerb()
    {
        var operation = new HeadObjectOperation(new HeadObjectRequest("bucket", "key"));

        var result = operation.BuildRequest(OperationTestHelpers.CreateContext());

        Assert.Equal(HttpMethod.Head, result.Method);
        Assert.Equal("/bucket/key", result.ResourcePath);
    }

    [Fact]
    public void ParseResponse_ReturnsMetadata()
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Content-Length"] = "123",
            ["Content-Type"] = "text/plain",
            ["Last-Modified"] = "Tue, 05 Dec 2023 10:11:12 GMT",
            ["ETag"] = "\"abc\"",
            ["x-oss-meta-owner"] = "team",
            ["Unrelated"] = "ignored"
        };
        var response = OperationTestHelpers.CreateResponse(string.Empty, HttpStatusCode.OK, headers);
        var operation = new HeadObjectOperation(new HeadObjectRequest("bucket", "key"));

        var result = operation.ParseResponse(response);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.Equal(123, result.ContentLength);
        Assert.Equal("text/plain", result.ContentType);
        Assert.Equal(new DateTimeOffset(2023, 12, 5, 10, 11, 12, TimeSpan.Zero), result.LastModified);
        Assert.Equal("\"abc\"", result.ETag);
        Assert.Single(result.Metadata);
        Assert.Equal("team", result.Metadata["x-oss-meta-owner"]);
    }
}
