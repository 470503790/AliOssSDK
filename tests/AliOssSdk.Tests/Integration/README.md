# Integration Tests

This directory contains integration tests that interact with a real Alibaba Cloud OSS service.

## Overview

The integration tests validate real-world scenarios including:
- **File Upload**: Upload files to OSS using `PutObject` and `PutObjectFromFile` methods
- **File Download**: Download files from OSS by key using `GetObject` method

Both synchronous and asynchronous versions of these operations are tested.

## Running Integration Tests

Integration tests require valid Alibaba Cloud OSS credentials and a bucket to run. If credentials are not configured, the tests will be skipped automatically.

### Configuration

Set the following environment variables before running the integration tests:

| Environment Variable | Description | Example |
|---------------------|-------------|---------|
| `OSS_ENDPOINT` | OSS endpoint URL | `https://oss-cn-hangzhou.aliyuncs.com` |
| `OSS_ACCESS_KEY_ID` | Alibaba Cloud Access Key ID | `LTAI4G...` |
| `OSS_ACCESS_KEY_SECRET` | Alibaba Cloud Access Key Secret | `your-secret-key` |
| `OSS_BUCKET_NAME` | Bucket name for testing | `test-bucket` |
| `OSS_REGION` | Region (optional if endpoint includes region) | `cn-hangzhou` |

### Example: Running Tests on Linux/macOS

```bash
# Set environment variables
export OSS_ENDPOINT="https://oss-cn-hangzhou.aliyuncs.com"
export OSS_ACCESS_KEY_ID="your-access-key-id"
export OSS_ACCESS_KEY_SECRET="your-access-key-secret"
export OSS_BUCKET_NAME="your-test-bucket"
export OSS_REGION="cn-hangzhou"

# Run integration tests
dotnet test --filter "FullyQualifiedName~Integration"
```

### Example: Running Tests on Windows (PowerShell)

```powershell
# Set environment variables
$env:OSS_ENDPOINT="https://oss-cn-hangzhou.aliyuncs.com"
$env:OSS_ACCESS_KEY_ID="your-access-key-id"
$env:OSS_ACCESS_KEY_SECRET="your-access-key-secret"
$env:OSS_BUCKET_NAME="your-test-bucket"
$env:OSS_REGION="cn-hangzhou"

# Run integration tests
dotnet test --filter "FullyQualifiedName~Integration"
```

### Example: Running Tests on Windows (Command Prompt)

```cmd
REM Set environment variables
set OSS_ENDPOINT=https://oss-cn-hangzhou.aliyuncs.com
set OSS_ACCESS_KEY_ID=your-access-key-id
set OSS_ACCESS_KEY_SECRET=your-access-key-secret
set OSS_BUCKET_NAME=your-test-bucket
set OSS_REGION=cn-hangzhou

REM Run integration tests
dotnet test --filter "FullyQualifiedName~Integration"
```

## Test Details

### PutObject_UploadsFileToOss_Successfully

**Synchronous test** that:
1. Creates a temporary file with test content
2. Uploads the file to OSS using `PutObjectFromFile`
3. Verifies the upload was successful (status code 200 or 201)
4. Verifies an ETag was returned
5. Cleans up by deleting the temporary file and the uploaded object

### GetObject_DownloadsFileFromOssByKey_Successfully

**Synchronous test** that:
1. Uploads a test file to OSS
2. Downloads the file by key using `GetObject`
3. Verifies the downloaded content matches the uploaded content
4. Cleans up by deleting the uploaded object

### PutObjectAsync_UploadsFileToOss_Successfully

**Asynchronous test** that performs the same operations as `PutObject_UploadsFileToOss_Successfully` but uses async/await.

### GetObjectAsync_DownloadsFileFromOssByKey_Successfully

**Asynchronous test** that performs the same operations as `GetObject_DownloadsFileFromOssByKey_Successfully` but uses async/await.

## Security Notes

- **Never commit credentials** to source control
- Use environment variables or secure credential management systems
- The test bucket should be dedicated for testing purposes
- Integration tests will create and delete objects with names like `test-upload-{guid}.txt` and `test-download-{guid}.txt`
- All tests include cleanup logic to delete created objects, even if the test fails

## Troubleshooting

### Tests are being skipped

If credentials are not configured, the tests will be skipped. Verify that all required environment variables are set correctly.

### Authentication errors

Ensure your Access Key ID and Secret are correct and have the necessary permissions to:
- Put objects to the bucket
- Get objects from the bucket
- Delete objects from the bucket

### Connection errors

Verify that:
- The endpoint URL is correct for your region
- Your network allows connections to Alibaba Cloud OSS
- The bucket exists and is accessible
