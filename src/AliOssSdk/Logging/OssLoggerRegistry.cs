namespace AliOssSdk.Logging
{
    /// <summary>
    /// Provides a process-wide logger registration so that applications can register
    /// a single <see cref="ILogger"/> instance that will be used whenever no logger
    /// is provided via configuration.
    /// </summary>
    public static class OssLoggerRegistry
    {
        private static ILogger _logger = NullLogger.Instance;

        /// <summary>
        /// Gets the globally registered logger instance.
        /// </summary>
        public static ILogger Logger => _logger;

        /// <summary>
        /// Registers the global logger. Passing <c>null</c> reverts back to
        /// <see cref="NullLogger.Instance"/>.
        /// </summary>
        /// <param name="logger">The logger to register.</param>
        public static void RegisterLogger(ILogger? logger)
        {
            _logger = logger ?? NullLogger.Instance;
        }
    }
}
