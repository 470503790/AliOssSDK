using System;

namespace AliOssSdk.Logging
{
    /// <summary>
    /// A minimal logging abstraction for the OSS client.
    /// </summary>
    public interface ILogger
    {
        void Log(OssLogEvent logEvent);
    }
}
