using Serilog.Context;
namespace VendorHub.Extensions
{
    public static class LoggerExtensions
    {
        private static void LogWithContext(string propertyName, object contextData, Action logAction)
        {
            using(LogContext.PushProperty(propertyName, contextData, destructureObjects: true))
            {
                logAction();
            }
        }
        public static void LogErrorWithContext(this ILogger logger, string messageTemplate, object errorObject, params object[] messageArgs)
        {
            LogWithContext("Error", errorObject, () => logger.LogError(messageTemplate, messageArgs));
        }

        public static void LogInfoWithContext(this ILogger logger, string messageTemplate, object payload, params object[] messageArgs)
        {
            LogWithContext("Payload", payload, () => logger.LogInformation(messageTemplate, messageArgs));
        }

        public static void LogWarningWithContext(this ILogger logger, string messageTemplate, object metadata, params object[] messageArgs)
        {
            LogWithContext("WarningDetails", metadata, () => logger.LogWarning(messageTemplate, messageArgs));
        }
    }
}
