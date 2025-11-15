# OSS API coverage tracker

Aliyun's [OSS API reference (按功能列出的操作)](https://help.aliyun.com/zh/oss/developer-reference/api-operations-by-feature) groups operations by scenario. The table below summarizes what this SDK currently supports and which surface areas are still pending so contributors can prioritize upcoming work.

| Category | Implemented | Pending (per Aliyun docs) |
| --- | --- | --- |
| Buckets | `ListBuckets`, `CreateBucket`, `DeleteBucket`, `GetBucketInfo`, `GetBucketAcl`, `PutBucketAcl` | `PutBucketLifecycle`, `GetBucketLifecycle`, `DeleteBucketLifecycle`, `PutBucketLogging`, `GetBucketLogging`, `PutBucketReferer`, `GetBucketReferer`, `PutBucketEncryption`, `GetBucketEncryption`, `DeleteBucketEncryption`, `PutBucketWebsite`, `GetBucketWebsite`, `DeleteBucketWebsite`, `PutBucketPolicy`, `GetBucketPolicy`, `DeleteBucketPolicy`, `GetBucketStat` |
| Objects | `PutObject`, `GetObject`, `DeleteObject`, `ListObjects`, `HeadObject`, `CopyObject` | `AppendObject`, `PostObject`, `PutObjectTagging`, `GetObjectTagging`, `DeleteObjectTagging`, `PutObjectAcl`, `GetObjectAcl`, `RestoreObject`, `PutObjectMeta` |
| Multipart uploads | `InitiateMultipartUpload`, `UploadPart`, `CompleteMultipartUpload`, `AbortMultipartUpload`, `ListParts`, `ListMultipartUploads` | `UploadPartCopy`, `ListMultipartUploadParts` |
| Permissions & security | `GetBucketAcl`, `PutBucketAcl` | `PutBucketPolicy`, `GetBucketPolicy`, `DeleteBucketPolicy`, `PutBucketEncryption`, `GetBucketEncryption`, `DeleteBucketEncryption`, `PutBucketTagging`, `GetBucketTagging`, `DeleteBucketTagging` |

> **How to contribute:** add DTOs under `src/AliOssSdk/Models`, implement `IOssOperation<T>` under `src/AliOssSdk/Operations`, then expose new helpers via `IOssClient` as needed. Use this checklist to avoid duplicating work and to keep parity with Alibaba Cloud's official specification.
