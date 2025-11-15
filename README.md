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

The client requires the OSS endpoint, access keys, and (optionally) a default region. The `OssClientConfiguration` object centralizes these settings and can be supplied through dependency injection or manual construction.

```csharp
var configuration = new OssClientConfiguration(
    new Uri("https://oss-cn-hangzhou.aliyuncs.com"),
    "<access-key-id>",
    "<access-key-secret>")
{
    DefaultRegion = "oss-cn-hangzhou",
    Logger = new ConsoleLogger(),          // optional
    HttpClient = new DefaultOssHttpClient(),// optional custom transport
    RequestSigner = new HmacSha1RequestSigner() // optional custom signer
};

var client = new OssClient(configuration);
```

Configuration can also be bound from `app.config`/`web.config` or an IOC container. At a minimum the endpoint and credentials must be provided.

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

Common end-to-end scenarios—including listing buckets, uploading objects, downloading objects, and deleting objects—are documented with both sync and async snippets in [`docs/usage.md`](docs/usage.md).

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

Set `configuration.Logger = new SerilogLogger(Log.Logger);` to enable your custom logging pipeline.

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
