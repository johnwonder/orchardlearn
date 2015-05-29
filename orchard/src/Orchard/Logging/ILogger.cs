using System;

namespace Orchard.Logging {
    /// <summary>
    /// ��־����
    /// </summary>
    public enum LogLevel {
        Debug,
        Information,
        Warning,
        Error,
        Fatal
    }

    /// <summary>
    /// ��־�ӿ�
    /// </summary>
    public interface ILogger {
        bool IsEnabled(LogLevel level);
        void Log(LogLevel level, Exception exception, string format, params object[] args);
    }
}
