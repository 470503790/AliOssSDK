namespace AliOssSdk.Logging
{
    public sealed class NullLogger : ILogger
    {
        public static NullLogger Instance { get; } = new NullLogger();

        private NullLogger()
        {
        }

        public void Log(OssLogEvent logEvent)
        {
            // Intentionally left blank.
        }
    }
}
