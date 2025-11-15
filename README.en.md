# AliOssSDK

A lightweight, dependency-injection friendly OSS client for .NET Framework 4.8. The solution models the core Alibaba Cloud OSS operations through composable operations and a configurable HTTP layer, enabling both synchronous and asynchronous workflows.

## Table of contents

1. [Projects](#projects)
2. [Prerequisites](#prerequisites)
3. [Installation](#installation)
4. [Configuration](#configuration)
5. [Usage overview](#usage-overview)
6. [Logging](#logging)
7. [Design and extensibility](#design-and-extensibility)
8. [Further examples](#further-examples)

## Projects

- **AliOssSdk** – Class library that exposes abstractions such as `IOssClient`, request/response DTOs, operations, and a pluggable HTTP/signing pipeline.

## Prerequisites

- **.NET Framework 4.8** – the SDK targets net48 so the developer box, CI, and deployment environments must have the reference assemblies installed.
- **Alibaba Cloud credentials** – obtain an Access Key ID/Secret pair with permissions for the buckets you intend to manage.
- **An OSS endpoint** – e.g., `https://oss-cn-hangzhou.aliyuncs.com`. Regional endpoints are documented in Alibaba Cloud's OSS docs.

## Installation

Install from NuGet:

```powershell
Install-Package AliOssSdk
```

Or add a `<PackageReference>` inside your project file:

```xml
<ItemGroup>
  <PackageReference Include="AliOssSdk" Version="1.x.x" />
</ItemGroup>
```

If you are consuming the repository directly, add the `src/AliOssSdk/AliOssSdk.csproj` project reference to your solution and build with Visual Studio 2019+ (net48 targeting pack required).

## Configuration

The client requires the OSS endpoint, access keys, and (optionally) default values such as the region or bucket. The `OssClientConfiguration` object centralizes these settings and can be supplied through dependency injection or manual construction.

```csharp
var configuration = new OssClientConfiguration(
    new Uri("https://oss-cn-hangzhou.aliyuncs.com"),
    "<access-key-id>",
    "<access-key-secret>")
{
    DefaultRegion = "cn-hangzhou",
    DefaultBucketName = "example-bucket",
    Logger = new ConsoleLogger(),          // optional
    HttpClient = new DefaultOssHttpClient(),// optional custom transport
    RequestSigner = new OssRequestSignerV4()    // optional custom signer (SigV4)
};

var client = new OssClient(configuration);
```

Configuration can also be bound from `app.config`/`web.config` or an IOC container. At a minimum the endpoint and credentials must be provided. When `DefaultBucketName` is set, helpers such as `HeadObjectRequest` can omit the bucket name and reuse the configured value.

> **Signature Version 4** – Starting with this release the SDK signs every request with the V4 algorithm (`OSS4-HMAC-SHA256`). The signer automatically infers the region from endpoints such as `https://oss-cn-hangzhou.aliyuncs.com`. If you are using a custom domain, set `DefaultRegion` (e.g., `cn-hangzhou`) explicitly so that the credential scope can be calculated.

## Usage overview

The SDK exposes synchronous and asynchronous helpers for every operation via `IOssClient`. Operations are modeled as self-contained request objects implementing `IOssOperation<TResponse>`, making it easy to add new capabilities without altering the client surface.

```csharp
// Async
var createBucket = new CreateBucketOperation(new CreateBucketRequest("example-bucket"));
await client.ExecuteAsync(createBucket);

// Sync
var listBuckets = new ListBucketsOperation();
var response = client.Execute(listBuckets);
```

When you prefer simpler entry points, `IOssClient` exposes typed helpers such as `GetObjectAsync`, `ListObjectsAsync`, and `InitiateMultipartUploadAsync` (plus their synchronous counterparts). These methods internally create the corresponding operations and reuse the same execution pipeline, so sync and async behavior remains consistent.

### Convenience helpers

In addition to the raw operations, the SDK now includes a couple of opinionated shortcuts:

- `PutObjectFromFile` / `PutObjectFromFileAsync`: upload by simply providing a file path. The client opens the stream for you and delegates to `PutObject`.
- `MoveObjects` / `MoveObjectsAsync`: move objects in batches by chaining `CopyObject` and `DeleteObject` calls under the hood.

```csharp
await client.PutObjectFromFileAsync("my-demo-bucket", "images/logo.png", "./logo.png", "image/png");

await client.MoveObjectsAsync(new[]
{
    new ObjectMoveDescriptor("my-demo-bucket", "raw/video.mov", "my-archive", "2024/video.mov"),
    new ObjectMoveDescriptor("my-demo-bucket", "tmp/report.pdf", "my-demo-bucket", "reports/2024/report.pdf")
});
```

Common end-to-end scenarios—including listing buckets, uploading objects, downloading objects, and deleting objects—are documented with both sync and async snippets in [`docs/usage.md`](docs/usage.md). The repository also tracks parity with Aliyun's official "按功能列出的操作" catalog in [`docs/operation-coverage.md`](docs/operation-coverage.md) so it is easy to spot which APIs still need contributions.

## Error handling

Every HTTP call flows through `OssHttpClient`. When OSS returns a non-success status code, the client throws an `AliOssSdk.Http.OssRequestException` that contains the status code, `x-oss-request-id`, headers, and the response body (if available). You can capture the exception in both async and sync workflows to inspect these details:

```csharp
using AliOssSdk.Http;

try
{
    await client.ExecuteAsync(new DeleteObjectOperation(new DeleteObjectRequest("demo", "missing.txt")));
}
catch (OssRequestException ex)
{
    Console.WriteLine($"Failed with {ex.StatusCode}, request ID {ex.RequestId}");
    Console.WriteLine(ex.ResponseBody);
}

try
{
    client.Execute(new GetObjectOperation(new GetObjectRequest("demo", "missing.txt")));
}
catch (OssRequestException ex)
{
    // Sync entry points surface the same exception type.
    Console.WriteLine($"Headers: {string.Join(",", ex.ResponseHeaders)}");
}
```

## Logging

`OssClientConfiguration` accepts any implementation of `AliOssSdk.Logging.ILogger`. The SDK includes `ConsoleLogger` and `NullLogger`; you can plug in your own adapter (e.g., Serilog, log4net) by implementing the single `Log(OssLogEvent logEvent)` method.

```csharp
public sealed class SerilogLogger : ILogger
{
    private readonly Serilog.ILogger _logger;

    public SerilogLogger(Serilog.ILogger logger) => _logger = logger;

    public void Log(OssLogEvent logEvent)
    {
        _logger
            .ForContext("Operation", logEvent.OperationName)
            .ForContext("Duration", logEvent.Duration)
            .Write(ConvertLevel(logEvent.EventType), logEvent.Message, logEvent.Exception);
    }
}
```

Set `configuration.Logger = new SerilogLogger(Log.Logger);` to enable your custom logging pipeline. You can also register a
process-wide default via `OssLoggerRegistry.RegisterLogger(new SerilogLogger(Log.Logger));` so that every `OssClient`
automatically picks it up when no logger is specified in `OssClientConfiguration`.

## Design and extensibility

AliOssSDK is intentionally small but expressive. A few core patterns underpin the implementation:

- **Operation pattern** – `IOssOperation<TResponse>` encapsulates the request/response mapping for an OSS action. You can add new operations by placing a class under `src/AliOssSdk/Operations` and wiring the required request payload DTOs.
- **Template method execution** – `OssClient` orchestrates serialization, signing, transport, and error handling in one place. Both `Execute` and `ExecuteAsync` reuse the same template to guarantee parity between sync and async flows.
- **Dependency injection friendly** – Configuration, HTTP transport (`IOssHttpClient`), and signing (`IOssRequestSigner`) are injected so you can swap implementations for testing or platform-specific behavior.
- **Logging hooks** – the `ILogger` abstraction surfaces every request lifecycle event in a consistent structure (`OssLogEvent`).

Because of these extensibility points, future contributors can:

- Introduce custom transports (e.g., `HttpClient`, `RestSharp`) by implementing `IOssHttpClient`.
- Extend signing strategies (STS, RAM roles) through `IOssRequestSigner`.
- Add new operations without touching `OssClient`—just implement the `BuildRequest`/`ParseResponse` contract.

## Further examples

For detailed walkthroughs—including bucket/object CRUD, streaming uploads, and error handling—see [`docs/usage.md`](docs/usage.md).
