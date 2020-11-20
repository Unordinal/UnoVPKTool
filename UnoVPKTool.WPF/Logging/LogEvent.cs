using System;

namespace UnoVPKTool.WPF.Logging
{
    public delegate void LogEventHandler(object? sender, LogEventArgs e);

    public class LogEventArgs : EventArgs
    {
        public string? Message { get; set; }

        public LogLevel Level { get; set; }

        public LogEventArgs(string? message, LogLevel level) : base()
        {
            Message = message;
            Level = level;
        }
    }
}