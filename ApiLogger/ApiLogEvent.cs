namespace dotnet_api_extensions.ApiLogger
{
    /// <summary>
    /// An API Log Event that stores the Request and Response Model
    /// </summary>
    public class ApiLogEvent
    {
        /// <summary>
        /// Time stamp of the event
        /// </summary>
        public DateTime TimeStamp { get; set; }

        /// <summary>
        /// Name of the file 
        /// </summary>		
        public string FileName { get; set; }

        /// <summary>
        /// Url of the API
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Status code of the response
        /// </summary>
        public string StatusCode { get; set; }

        /// <summary>
        /// Request Body of the Request
        /// </summary>
        public object RequestBody { get; set; }

        /// <summary>
        /// Response body of the API
        /// </summary>
        public object ResponseBody { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ApiLogEvent()
        {
            FileName = $"{Guid.NewGuid()}.json";
            TimeStamp = DateTime.UtcNow;
        }

        /// <summary>
        /// Get the directory path based on the Timestamp
        /// </summary>
        /// <returns></returns>
        public string DirectoryPath => Path.Combine(
                    TimeStamp.Year.ToString(),
                    TimeStamp.Month.ToString(),
                    TimeStamp.Day.ToString(),
                    TimeStamp.Hour.ToString(),
                    TimeStamp.Minute.ToString());

        /// <summary>
        /// Get complete file path, including directory path
        /// </summary>
        public string FilePath => Path.Combine(DirectoryPath, FileName);
    }
}
