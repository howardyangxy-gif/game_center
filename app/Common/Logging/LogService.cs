using Serilog;
using System.Text.Json;
using ILogger = Serilog.ILogger;

namespace app.Common.Logging
{
    public static class LogService
    {
        private static readonly ILogger _agentLogger = Log.ForContext("ServiceName", "Agent");
        private static readonly ILogger _gameLogger = Log.ForContext("ServiceName", "Game");
        private static readonly ILogger _playerLogger = Log.ForContext("ServiceName", "Player");
        private static readonly ILogger _systemLogger = Log.ForContext("ServiceName", "System");

        public static class Agent
        {
            public static void LogInfo(string message, object? data = null)
                => LogMessage(_agentLogger, LogLevel.Information, message, data);

            public static void LogWarning(string message, object? data = null)
                => LogMessage(_agentLogger, LogLevel.Warning, message, data);

            public static void LogError(string message, Exception? ex = null, object? data = null)
                => LogMessage(_agentLogger, LogLevel.Error, message, data, ex);

            public static void LogDebug(string message, object? data = null)
                => LogMessage(_agentLogger, LogLevel.Debug, message, data);
        }

        public static class Game
        {
            public static void LogInfo(string message, object? data = null)
                => LogMessage(_gameLogger, LogLevel.Information, message, data);

            public static void LogWarning(string message, object? data = null)
                => LogMessage(_gameLogger, LogLevel.Warning, message, data);

            public static void LogError(string message, Exception? ex = null, object? data = null)
                => LogMessage(_gameLogger, LogLevel.Error, message, data, ex);

            public static void LogDebug(string message, object? data = null)
                => LogMessage(_gameLogger, LogLevel.Debug, message, data);
        }

        public static class Player
        {
            public static void LogInfo(string message, object? data = null)
                => LogMessage(_playerLogger, LogLevel.Information, message, data);

            public static void LogWarning(string message, object? data = null)
                => LogMessage(_playerLogger, LogLevel.Warning, message, data);

            public static void LogError(string message, Exception? ex = null, object? data = null)
                => LogMessage(_playerLogger, LogLevel.Error, message, data, ex);

            public static void LogDebug(string message, object? data = null)
                => LogMessage(_playerLogger, LogLevel.Debug, message, data);
        }

        public static class System
        {
            public static void LogInfo(string message, object? data = null)
                => LogMessage(_systemLogger, LogLevel.Information, message, data);

            public static void LogWarning(string message, object? data = null)
                => LogMessage(_systemLogger, LogLevel.Warning, message, data);

            public static void LogError(string message, Exception? ex = null, object? data = null)
                => LogMessage(_systemLogger, LogLevel.Error, message, data, ex);

            public static void LogDebug(string message, object? data = null)
                => LogMessage(_systemLogger, LogLevel.Debug, message, data);
        }

        private static void LogMessage(ILogger logger, LogLevel level, string message, object? data = null, Exception? exception = null)
        {
            var logData = new
            {
                Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                Level = level.ToString(),
                Message = message,
                Data = data
            };

            switch (level)
            {
                case LogLevel.Information:
                    if (exception != null)
                        logger.Information(exception, "{@LogData}", logData);
                    else
                        logger.Information("{@LogData}", logData);
                    break;
                case LogLevel.Warning:
                    if (exception != null)
                        logger.Warning(exception, "{@LogData}", logData);
                    else
                        logger.Warning("{@LogData}", logData);
                    break;
                case LogLevel.Error:
                    if (exception != null)
                        logger.Error(exception, "{@LogData}", logData);
                    else
                        logger.Error("{@LogData}", logData);
                    break;
                case LogLevel.Debug:
                    if (exception != null)
                        logger.Debug(exception, "{@LogData}", logData);
                    else
                        logger.Debug("{@LogData}", logData);
                    break;
            }
        }

        public enum LogLevel
        {
            Debug,
            Information,
            Warning,
            Error
        }
    }
}