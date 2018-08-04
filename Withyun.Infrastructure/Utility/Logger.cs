using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Withyun.Infrastructure.Utility
{
    public class Logger 
    {
        public static ILogger<Logger> logger;

        public static void Info(string module, string message, params object[] args)
        {
            logger.LogInformation(message, args);
        }

        public static void Trace(string module, string message, params object[] args)
        {
            logger.LogTrace(message, args);
        }

        public static void Error(Exception ex, string message, params object[] args)
        {
            logger.LogError(ex, message, args);
        }
        public static void Error(Exception ex)
        {
            logger.LogError(ex, "");
        }

        public static void Email(string message, params object[] args)
        {
            logger.LogWarning(message, args);
        }

        public static void Alert(string message, params object[] args)
        {
            logger.LogCritical(message, args);
        }
    }
}
