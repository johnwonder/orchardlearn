using System;

namespace Orchard.Logging {
    /// <summary>
    /// 日志级别
    /// </summary>
    public enum LogLevel {
        Debug,
        Information,
        Warning,
        Error,
        Fatal
    }

    /// <summary>
    /// 日志接口
    /// </summary>
    public interface ILogger {
        bool IsEnabled(LogLevel level);
        void Log(LogLevel level, Exception exception, string format, params object[] args);
    }
}
