# 集成测试

本目录包含与真实的阿里云 OSS 服务交互的集成测试。

## 概述

集成测试验证以下真实场景：
- **文件上传**：使用 `PutObject` 和 `PutObjectFromFile` 方法上传文件到 OSS
- **文件下载**：使用 `GetObject` 方法通过 key 从 OSS 下载文件

同步和异步版本的操作都经过了测试。

## 运行集成测试

集成测试需要有效的阿里云 OSS 凭证和存储桶才能运行。如果未配置凭证，测试将自动跳过。

### 配置

在运行集成测试之前，请设置以下环境变量：

| 环境变量 | 说明 | 示例 |
|---------|------|------|
| `OSS_ENDPOINT` | OSS 终端节点 URL | `https://oss-cn-hangzhou.aliyuncs.com` |
| `OSS_ACCESS_KEY_ID` | 阿里云访问密钥 ID | `LTAI4G...` |
| `OSS_ACCESS_KEY_SECRET` | 阿里云访问密钥 Secret | `your-secret-key` |
| `OSS_BUCKET_NAME` | 用于测试的存储桶名称 | `test-bucket` |
| `OSS_REGION` | 区域（如果终端节点包含区域则可选） | `cn-hangzhou` |

### 示例：在 Linux/macOS 上运行测试

```bash
# 设置环境变量
export OSS_ENDPOINT="https://oss-cn-hangzhou.aliyuncs.com"
export OSS_ACCESS_KEY_ID="your-access-key-id"
export OSS_ACCESS_KEY_SECRET="your-access-key-secret"
export OSS_BUCKET_NAME="your-test-bucket"
export OSS_REGION="cn-hangzhou"

# 运行集成测试
dotnet test --filter "FullyQualifiedName~Integration"
```

### 示例：在 Windows (PowerShell) 上运行测试

```powershell
# 设置环境变量
$env:OSS_ENDPOINT="https://oss-cn-hangzhou.aliyuncs.com"
$env:OSS_ACCESS_KEY_ID="your-access-key-id"
$env:OSS_ACCESS_KEY_SECRET="your-access-key-secret"
$env:OSS_BUCKET_NAME="your-test-bucket"
$env:OSS_REGION="cn-hangzhou"

# 运行集成测试
dotnet test --filter "FullyQualifiedName~Integration"
```

### 示例：在 Windows (命令提示符) 上运行测试

```cmd
REM 设置环境变量
set OSS_ENDPOINT=https://oss-cn-hangzhou.aliyuncs.com
set OSS_ACCESS_KEY_ID=your-access-key-id
set OSS_ACCESS_KEY_SECRET=your-access-key-secret
set OSS_BUCKET_NAME=your-test-bucket
set OSS_REGION=cn-hangzhou

REM 运行集成测试
dotnet test --filter "FullyQualifiedName~Integration"
```

## 测试详情

### PutObject_UploadsFileToOss_Successfully

**同步测试**，执行以下操作：
1. 创建带有测试内容的临时文件
2. 使用 `PutObjectFromFile` 上传文件到 OSS
3. 验证上传成功（状态码 200 或 201）
4. 验证返回了 ETag
5. 清理临时文件和已上传的对象

### GetObject_DownloadsFileFromOssByKey_Successfully

**同步测试**，执行以下操作：
1. 上传测试文件到 OSS
2. 使用 `GetObject` 通过 key 下载文件
3. 验证下载的内容与上传的内容匹配
4. 清理已上传的对象

### PutObjectAsync_UploadsFileToOss_Successfully

**异步测试**，执行与 `PutObject_UploadsFileToOss_Successfully` 相同的操作，但使用 async/await。

### GetObjectAsync_DownloadsFileFromOssByKey_Successfully

**异步测试**，执行与 `GetObject_DownloadsFileFromOssByKey_Successfully` 相同的操作，但使用 async/await。

## 安全注意事项

- **切勿将凭证提交到源代码控制**
- 使用环境变量或安全的凭证管理系统
- 测试存储桶应专用于测试目的
- 集成测试将创建和删除名称类似 `test-upload-{guid}.txt` 和 `test-download-{guid}.txt` 的对象
- 所有测试都包含清理逻辑，即使测试失败也会删除创建的对象

## 故障排除

### 测试被跳过

如果未配置凭证，测试将被跳过。请验证所有必需的环境变量是否正确设置。

### 身份验证错误

确保您的访问密钥 ID 和 Secret 正确，并且具有以下必要权限：
- 向存储桶放置对象
- 从存储桶获取对象
- 从存储桶删除对象

### 连接错误

验证：
- 您的区域的终端节点 URL 正确
- 您的网络允许连接到阿里云 OSS
- 存储桶存在且可访问

## 常用 OSS 区域

- 中国（杭州）：`https://oss-cn-hangzhou.aliyuncs.com`
- 中国（上海）：`https://oss-cn-shanghai.aliyuncs.com`
- 中国（北京）：`https://oss-cn-beijing.aliyuncs.com`
- 中国（深圳）：`https://oss-cn-shenzhen.aliyuncs.com`
- 美国（硅谷）：`https://oss-us-west-1.aliyuncs.com`
- 新加坡：`https://oss-ap-southeast-1.aliyuncs.com`

完整列表请参阅：https://help.aliyun.com/zh/oss/user-guide/regions-and-endpoints
