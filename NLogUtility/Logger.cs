using System;

namespace NLogUtility
{
    public class Logger
    {
        public static void Info(string module, string message, params object[] args)
        {
            NLog.Logger logger = NLog.LogManager.GetLogger(module);
            logger.Info(message, args);
        }

        public static void Trace(string module, string message, params object[] args)
        {
            NLog.Logger logger = NLog.LogManager.GetLogger(module);
            logger.Trace(message, args);
        }

        public static void Error(Exception ex, string message, params object[] args)
        {
            NLog.Logger logger = NLog.LogManager.GetLogger("error");
            logger.Error(ex, message, args);
        }
        public static void Error(Exception ex)
        {
            NLog.Logger logger = NLog.LogManager.GetLogger("error");
            logger.Error(ex);
        }

        public static void Email(string message, params object[] args)
        {
            NLog.Logger logger = NLog.LogManager.GetLogger("email");
            logger.Warn(message, args);
        }

        public static void Alert(string message, params object[] args)
        {
            NLog.Logger logger = NLog.LogManager.GetLogger("alert");
            logger.Fatal(message, args);
        }
    }
}
