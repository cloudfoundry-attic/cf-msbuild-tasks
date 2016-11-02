namespace CloudFoundry.Build.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using NLog;
    using NLog.Config;
    using NLog.Targets;

    internal static class FileLogger
    {
        private static NLog.Logger fileLogger = LogManager.GetLogger(System.AppDomain.CurrentDomain.FriendlyName);

        static FileLogger()
        {
            using (var fileTarget = new FileTarget())
            {
                string logFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                fileTarget.FileName = System.IO.Path.Combine(logFolder, "CloudFoundry", "cfmsbuild-${shortdate}.log");
                fileTarget.Layout = "${longdate} ${uppercase:${level}} ${message}";
                var config = new LoggingConfiguration();
                config.AddTarget("file", fileTarget);
                var rule = new LoggingRule("*", LogLevel.Debug, fileTarget);
                config.LoggingRules.Add(rule);
                LogManager.Configuration = config;
            }
        }

        public static void LogMessage(string message)
        {
            var callerMethod = new System.Diagnostics.StackTrace().GetFrame(2).GetMethod();
            string callerClass = callerMethod.ReflectedType.Name;
            fileLogger.Log(LogLevel.Debug, string.Join(Environment.NewLine, string.Empty, "Caller Class:" + callerClass, "Caller Method:" + callerMethod.Name, "Message:" + message));
        }

        public static void LogError(string message, Exception ex)
        {
            var callerMethod = new System.Diagnostics.StackTrace().GetFrame(2).GetMethod();
            string callerClass = callerMethod.ReflectedType.Name;

            fileLogger.Log(LogLevel.Error, ex, string.Join(Environment.NewLine, string.Empty, "Caller Class:" + callerClass, "Caller Method:" + callerMethod.Name, "Message:" + message));
        }
    }
}
