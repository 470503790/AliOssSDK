# AliOssSDK

**简体中文 | [English](README.en.md)**

一个面向 .NET Framework 4.8 的轻量级 OSS 客户端，遵循依赖注入友好的设计。解决方案通过可组合的 Operation 模式和可配置的 HTTP 层来抽象阿里云 OSS 的核心操作，确保同步与异步调用具有一致的执行流程。

## 目录

1. [项目组成](#项目组成)
2. [先决条件](#先决条件)
3. [安装](#安装)
4. [配置](#配置)
5. [使用概览](#使用概览)
6. [已实现的 OSS 操作](#已实现的-oss-操作)
7. [日志](#日志)
8. [设计与可扩展性](#设计与可扩展性)
9. [更多示例](#更多示例)

## 项目组成

- **AliOssSdk** – 类库，暴露 `IOssClient`、请求/响应 DTO、Operation 以及可插拔的 HTTP/签名管道。

## 先决条件

- **.NET Framework 4.8** – SDK 目标框架为 net48，开发机、CI 与部署环境都需要安装对应的 Reference Assemblies。
- **阿里云 AccessKey** – 准备具备目标 Bucket 权限的 Access Key ID / Secret。
- **OSS Endpoint** – 例如 `https://oss-cn-hangzhou.aliyuncs.com`，可在阿里云文档中查阅地域 Endpoint 列表。

## 安装

通过 NuGet 安装：

```powershell
Install-Package AliOssSdk
```

或在项目文件中添加 `<PackageReference>`：

```xml
<ItemGroup>
  <PackageReference Include="AliOssSdk" Version="1.x.x" />
</ItemGroup>
```

如果直接引用源码仓库，可在解决方案中添加 `src/AliOssSdk/AliOssSdk.csproj` 引用，并使用 Visual Studio 2019+（需安装 net48 targeting pack）进行构建。

## 配置

客户端需要 Endpoint、访问密钥以及可选的默认地域。`OssClientConfiguration` 对象集中管理这些设置，可通过依赖注入或手动创建传入。

```csharp
var configuration = new OssClientConfiguration(
    new Uri("https://oss-cn-hangzhou.aliyuncs.com"),
    "<access-key-id>",
    "<access-key-secret>")
{
    DefaultRegion = "cn-hangzhou",
    Logger = new ConsoleLogger(),          // 可选
    HttpClient = new DefaultOssHttpClient(),// 可选自定义传输层
    RequestSigner = new OssRequestSignerV4()    // 可选自定义签名器（SigV4）
};

var client = new OssClient(configuration);
```

配置同样可以从 `app.config`/`web.config` 绑定，或交由 IOC 容器注入。最少需要提供 Endpoint 与凭证。

### 通过 JSON/环境变量加载配置

`AlibabaOssConfig` 提供了一个可以与 JSON 文件或环境变量绑定的 POCO，方便在部署环境中集中管理访问密钥。

```json
{
  "endpoint": "https://oss-cn-hangzhou.aliyuncs.com",
  "region": "cn-hangzhou",
  "bucket": "demo",
  "accessKeyId": "<key>",
  "accessKeySecret": "<secret>",
  "sign_duration_second": 3600
}
```

可以在启动代码中读取该文件，并允许使用 `ALI_OSS_` 前缀的环境变量覆写同名字段：

> 如果配置中未提供 `endpoint` 或值为空，SDK 会自动回落到默认域名 `https://oss-cn-hangzhou.aliyuncs.com`。

```csharp
var alibabaConfig = AlibabaOssConfig
    .FromJsonFile("osssettings.json")
    .ApplyEnvironmentOverrides();

var client = new OssClient(alibabaConfig.ToOssClientConfiguration());
```

上例中如果同时设置了 `ALI_OSS_ACCESS_KEY_SECRET` 等环境变量，将优先生效，便于区分开发/生产环境。

> **签名算法升级**：SDK 现默认使用 V4 (`OSS4-HMAC-SHA256`) 签名。若 Endpoint 中包含地域（如 `https://oss-cn-hangzhou.aliyuncs.com`），签名器会自动解析；若使用自定义域名，请显式设置 `DefaultRegion`（例如 `cn-hangzhou`）以生成正确的 Credential Scope。

## 使用概览

SDK 通过 `IOssClient` 为每个 Operation 提供同步 (`Execute`) 与异步 (`ExecuteAsync`) 执行入口，Operation 实现 `IOssOperation<TResponse>`，可独立扩展而无需修改客户端表面。

```csharp
// 异步
var createBucket = new CreateBucketOperation(new CreateBucketRequest("example-bucket"));
await client.ExecuteAsync(createBucket);

// 同步
var listBuckets = new ListBucketsOperation();
var response = client.Execute(listBuckets);
```

同时 `IOssClient` 还提供 `GetObjectAsync`、`ListObjectsAsync`、`InitiateMultipartUploadAsync` 及其同步同名方法等语法糖，它们内部同样创建并执行对应 Operation，保证同步/异步一致性。

完整的端到端场景（列举 Bucket、上传/下载对象、删除对象等）在 [中文使用指南](docs/usage.zh-CN.md) 与 [English guide](docs/usage.md) 中提供同步与异步示例。

## 错误处理

所有 HTTP 调用都会经过 `OssHttpClient`。当 OSS 返回非 2xx 状态码时，客户端会抛出 `AliOssSdk.Http.OssRequestException`，其中包含 HTTP 状态码、`x-oss-request-id`、响应头以及（若存在）响应体。可以在同步/异步场景中捕获该异常并读取详细信息：

```csharp
using AliOssSdk.Http;

try
{
    await client.ExecuteAsync(new DeleteObjectOperation(new DeleteObjectRequest("demo", "missing.txt")));
}
catch (OssRequestException ex)
{
    Console.WriteLine($"状态码: {ex.StatusCode}, 请求 ID: {ex.RequestId}");
    Console.WriteLine(ex.ResponseBody);
}

try
{
    client.Execute(new GetObjectOperation(new GetObjectRequest("demo", "missing.txt")));
}
catch (OssRequestException ex)
{
    // 同步入口同样抛出该异常类型
    Console.WriteLine($"响应头: {string.Join(",", ex.ResponseHeaders)}");
}
```

## 已实现的 OSS 操作

下表列出目前 SDK 已实现的 Operation 以及可直接调用的同步/异步方法。所有 Operation 同样支持 `Execute`/`ExecuteAsync` 通用入口。

| 分类 | Operation | 同步方法 | 异步方法 |
| --- | --- | --- | --- |
| Bucket | `ListBuckets` | `IOssClient.ListBuckets` | `IOssClient.ListBucketsAsync` |
| Bucket | `CreateBucket` | `IOssClient.CreateBucket` | `IOssClient.CreateBucketAsync` |
| Bucket | `DeleteBucket` | `IOssClient.DeleteBucket` | `IOssClient.DeleteBucketAsync` |
| Bucket | `GetBucketInfo` | `IOssClient.GetBucketInfo` | `IOssClient.GetBucketInfoAsync` |
| Bucket | `GetBucketAcl` | `IOssClient.GetBucketAcl` | `IOssClient.GetBucketAclAsync` |
| Bucket | `PutBucketAcl` | `IOssClient.PutBucketAcl` | `IOssClient.PutBucketAclAsync` |
| Object | `PutObject` | `IOssClient.PutObject` | `IOssClient.PutObjectAsync` |
| Object | `GetObject` | `IOssClient.GetObject` | `IOssClient.GetObjectAsync` |
| Object | `DeleteObject` | `IOssClient.DeleteObject` | `IOssClient.DeleteObjectAsync` |
| Object | `ListObjects` | `IOssClient.ListObjects` | `IOssClient.ListObjectsAsync` |
| Object | `HeadObject` | `IOssClient.HeadObject` | `IOssClient.HeadObjectAsync` |
| Object | `CopyObject` | `IOssClient.CopyObject` | `IOssClient.CopyObjectAsync` |
| Multipart | `InitiateMultipartUpload` | `IOssClient.InitiateMultipartUpload` | `IOssClient.InitiateMultipartUploadAsync` |
| Multipart | `UploadPart` | `IOssClient.UploadPart` | `IOssClient.UploadPartAsync` |
| Multipart | `CompleteMultipartUpload` | `IOssClient.CompleteMultipartUpload` | `IOssClient.CompleteMultipartUploadAsync` |
| Multipart | `AbortMultipartUpload` | `IOssClient.AbortMultipartUpload` | `IOssClient.AbortMultipartUploadAsync` |
| Multipart | `ListParts` | `IOssClient.ListParts` | `IOssClient.ListPartsAsync` |
| Multipart | `ListMultipartUploads` | `IOssClient.ListMultipartUploads` | `IOssClient.ListMultipartUploadsAsync` |

待实现/可贡献的 Operation 清单请查看 [`docs/operation-coverage.md`](docs/operation-coverage.md)。

## 日志

`OssClientConfiguration` 接受任何 `AliOssSdk.Logging.ILogger` 实现。SDK 默认提供 `ConsoleLogger` 与 `NullLogger`，你也可以像下例一样适配 Serilog、log4net 等框架：

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

在配置中设置 `configuration.Logger = new SerilogLogger(Log.Logger);` 即可启用自定义日志管道。

## 设计与可扩展性

AliOssSDK 体积小但表达力强，核心模式如下：

- **Operation 模式** – `IOssOperation<TResponse>` 封装单个 OSS 行为的请求/响应映射。添加新 Operation 时只需在 `src/AliOssSdk/Operations` 下放置类并提供所需 DTO。
- **模板方法执行** – `OssClient` 集中处理序列化、签名、网络传输与错误处理，`Execute` 与 `ExecuteAsync` 共用模板，确保同步/异步行为一致。
- **依赖注入友好** – 配置、HTTP 传输 (`IOssHttpClient`) 与签名 (`IOssRequestSigner`) 均为可注入抽象，方便测试或替换平台特定实现。
- **日志钩子** – `ILogger` 抽象会以统一的 `OssLogEvent` 结构曝光每个请求生命周期事件。

因此贡献者可以：

- 通过实现 `IOssHttpClient` 引入自定义传输（如 `HttpClient`、`RestSharp`）。
- 通过 `IOssRequestSigner` 扩展 STS、RAM 等签名策略。
- 在无需修改 `OssClient` 的前提下，通过实现 `BuildRequest`/`ParseResponse` 来添加新 Operation。

## 更多示例

包含 Bucket/Object CRUD、流式上传、错误处理等更完整的示例请参阅：[中文使用指南](docs/usage.zh-CN.md) 与 [English usage guide](docs/usage.md)。
