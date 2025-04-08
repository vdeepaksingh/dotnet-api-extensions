using Newtonsoft.Json;

using System.Collections.Concurrent;

namespace dotnet_api_extensions.ApiLogger
{
    /// <summary>
    /// Writes the API log to a file
    /// </summary>
    public class ApiLogWriter : IApiLogWriter
    {
        private readonly BlockingCollection<ApiLogEvent> _queue;
        private readonly Task _worker;
        private readonly ILogger<ApiLogWriter> _logger;
        private readonly string _baseDirectoryPath;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="configuration"></param>
        public ApiLogWriter(ILogger<ApiLogWriter> logger, IConfiguration configuration)
        {

            _queue = new BlockingCollection<ApiLogEvent>();
            _worker = Task.Factory.StartNew(LogAction, CancellationToken.None,
                TaskCreationOptions.LongRunning | TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
            _logger = logger;

            var loggingSection = configuration.GetSection("Logging");
            _baseDirectoryPath = loggingSection["DirectoryPathForApiLogs"];

            if (string.IsNullOrEmpty(_baseDirectoryPath))
            {
                throw new ArgumentNullException("Logging:DirectoryPathForApiLogs", "DirectoryPathForApiLogs is not configured");
            }
            else
            {
                _baseDirectoryPath = Serilog.LoggerExtensions.GetDirectoryPath(_baseDirectoryPath, loggingSection);
            }
        }

        /// <summary>
        /// Log an event
        /// </summary>
        /// <param name="logEvent"></param>
        public void LogEvent(ApiLogEvent logEvent)
        {
            if (_queue.IsAddingCompleted)
            {
                return;
            }

            try
            {
                if (!_queue.TryAdd(logEvent))
                {
                    _logger.LogWarning("{summary} - Unable to Enqueue, capacity {queueCapacity}", "Failed to log Api Request", _queue.BoundedCapacity);
                }
            }
            catch (InvalidOperationException)
            {
                // Thrown in the event of a race condition when we try to add another event after
                // CompleteAdding has been called
                _logger.LogWarning("{summary} - CompleteAdding has been called", "Failed to log Api Request");
            }

        }

        /// <summary>
        /// IDisposable implementation
        /// </summary>
        public void Dispose()
        {
            // Prevent any more events from being added
            _queue.CompleteAdding();

            // Allow queued events to be flushed
            _worker.Wait();

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Writes the log file that have been queued
        /// </summary>
        private void LogAction()
        {
            try
            {
                foreach (var logEvent in _queue.GetConsumingEnumerable())
                {
                    try
                    {
                        var directoryPath = Path.Combine(_baseDirectoryPath, logEvent.DirectoryPath);
                        if (!Directory.Exists(directoryPath))
                        {
                            Directory.CreateDirectory(directoryPath);
                        }

                        File.WriteAllTextAsync(Path.Combine(_baseDirectoryPath, logEvent.FilePath),
                            JsonConvert.SerializeObject(logEvent));
                        _logger.LogDebug("LOGEVENT PUBLISHED {fileName}", logEvent.FileName);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "{summary}", "Failed to write API Log to file");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "{summary}", "Fatal error in BackgroundWorker thread.");
            }
        }
    }
}
