
namespace Nullspace
{
    public class LoggerConfig
    {
        public int FileMaxSize { get; set; } // kb
        public LogLevel mLogLevel { get; set; }
        public string FilePath { get; set; }
        public string Directory { get; set; }
        public string FileName { get; set; }
        public string FileExtention { get; set; }
        public int FlushInterval { get; set; }
    }
}
