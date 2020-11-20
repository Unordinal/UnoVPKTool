using System;

namespace UnoVPKTool.WPF.Logging
{
    public interface ILogger
    {
        public event LogEventHandler? LogUpdated;

        public void Log(object? message, LogLevel level);
    }
}