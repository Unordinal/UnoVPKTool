using System;
using System.IO;
using System.Reflection;

namespace UnoVPKTool.WPF.Logging
{
    public class Logger : ILogger
    {
        private bool _wasWrittenTo = false;

        public event LogEventHandler? LogUpdated;

        public string LogFileName { get; set; }

        public Logger(string logFileName = "log.txt")
        {
            LogFileName = logFileName;
        }

        private void InitForFirstLog()
        {
            _wasWrittenTo = true;
            bool fileExisted = File.Exists(LogFileName); // Mainly used as a quick way to tell if there's a log already existing. If one does, we assume it has at least one entry.
            if (fileExisted && new FileInfo(LogFileName).Length > 2000000) // Over 2 MB.
            {
                File.WriteAllBytes(LogFileName, Array.Empty<byte>());
                fileExisted = false; // No entries now!
            }

            using var writer = File.AppendText(LogFileName);
            string timestamp = DateTimeOffset.UtcNow.ToString("u");
            var assembly = Assembly.GetExecutingAssembly().GetName();

            if (fileExisted) writer.WriteLine();
            writer.WriteLine($"{assembly.Name} v{assembly.Version} - {timestamp}");
            writer.WriteLine(new string('-', 25));
        }

        public void Log(object? message, LogLevel level)
        {
            if (!_wasWrittenTo) InitForFirstLog();

            string messageWithLevel = $"[{level}]: {message}";
#if RELEASE
            if (level is not LogLevel.Debug)
#endif
            {
                Console.WriteLine(messageWithLevel);

                string timestamp = DateTimeOffset.UtcNow.ToString("HH:mm:ss.fff");
                using var writer = File.AppendText(LogFileName);
                writer.WriteLine($"[{timestamp}] {messageWithLevel}");
            }

            OnLogUpdated(message?.ToString(), level);
        }

        public void LogDebug(object? message)
        {
            Log(message, LogLevel.Debug);
        }

        public void LogInfo(object? message)
        {
            Log(message, LogLevel.Info);
        }

        public void LogMessage(object? message)
        {
            Log(message, LogLevel.Message);
        }

        public void LogWarning(object? message)
        {
            Log(message, LogLevel.Warning);
        }

        public void LogError(object? message)
        {
            Log(message, LogLevel.Error);
        }

        public void LogFatal(object? message)
        {
            Log(message, LogLevel.Fatal);
        }

        protected void OnLogUpdated(string? message, LogLevel level)
        {
            LogUpdated?.Invoke(this, new LogEventArgs(message, level));
        }
    }
}