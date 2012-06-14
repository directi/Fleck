using System;
using Tasks;

namespace Fleck
{
    public enum LogLevel
    {
        Debug,
        Info,
        Warn,
        Error
    }
    public interface LogWriter {
        void LogToFile(string str);
    }

    public class FleckLog
    {
        public static LogLevel Level = LogLevel.Info;
        private static LogWriter writer;

        public static Action<LogLevel, string, Exception> LogAction = (level, message, ex) =>
        {
            if (level >= Level)
            {
                var line = string.Format("{0} [{1}] {2} {3}", DateTime.Now, level, message, ex);
                if(writer != null){
                    writer.LogToFile(line);
                }else{
                    Console.WriteLine(line);
                }
            }
        };

        public static void SetLogWriter(LogWriter writer_){
            writer = writer_;
        }

        public static void Warn(string message, Exception ex)
        {
            LogAction(LogLevel.Warn, message, ex);
        }

        public static void Error(string message, Exception ex)
        {
            LogAction(LogLevel.Error, message, ex);
        }

        public static void Debug(string message, Exception ex)
        {
            LogAction(LogLevel.Debug, message, ex);
        }

        public static void Info(string message, Exception ex)
        {
            LogAction(LogLevel.Info, message, ex);
        }

    }
}
