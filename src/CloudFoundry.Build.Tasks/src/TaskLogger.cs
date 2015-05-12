namespace CloudFoundry.Build.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;
    using NLog;
    using NLog.Config;
    using NLog.Targets;
    using NLog.Targets.Wrappers;

    internal class TaskLogger
    {
        private static readonly NLog.Logger Log = LogManager.GetLogger(System.AppDomain.CurrentDomain.FriendlyName);
    
        private static readonly object ConfigureLock = new object();
        
        private static bool configured = false;
        
        private static string logFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "cf-msbuild-tasks", "cf-msbuild-tasks.log");
        
        private TaskLoggingHelper logger;
    
        public TaskLogger(ITask task)
        {
            this.logger = new TaskLoggingHelper(task);
        }

        internal void LogError(string message, Exception exception, params object[] args)
        {
            if (exception != null)
            {
                string msg = string.Format(CultureInfo.InvariantCulture, message, args);
                List<string> messages = new List<string>();
                messages.Add(msg);
                FormatExceptionMessage(exception, messages);
                this.logger.LogError(string.Join(Environment.NewLine, messages));
                this.LogExceptionVerbose(msg, exception);
            }
            else
            {
                this.logger.LogError(message, args);
                Configure();
                Log.Error(CultureInfo.InvariantCulture, message, args);
            }
        }

        internal void LogError(string message, params object[] args)
        {
            this.LogError(message, null, args);
        }

        internal void LogMessage(string message, params object[] args)
        {
            this.logger.LogMessage(message, args);
            Configure();
            Log.Info(CultureInfo.InvariantCulture, message, args);
        }

        internal void LogWarning(string message, params object[] args)
        {
            this.logger.LogWarning(message, args);
            Configure();
            Log.Warn(CultureInfo.InvariantCulture, message, args);
        }

        internal void LogErrorFromException(Exception ex)
        {
            this.logger.LogErrorFromException(ex);
            Configure();
            Log.ErrorException(ex.Message, ex);
        }

        private static void FormatExceptionMessage(Exception ex, List<string> message)
        {
            if (ex.GetType() == typeof(AggregateException))
            {
                foreach (Exception iex in (ex as AggregateException).Flatten().InnerExceptions)
                {
                    FormatExceptionMessage(iex, message);
                }
            }
            else
            {
                message.Add(ex.Message);

                if (ex.InnerException != null)
                {
                    FormatExceptionMessage(ex.InnerException, message);
                }
            }
        }

        private static void Configure()
        {
            if (configured)
            {
                return;
            }

            lock (ConfigureLock)
            {
                if (configured)
                {
                    return;
                }

                string directory = Path.GetDirectoryName(logFile);
                Directory.CreateDirectory(directory);

                FileTarget target = new FileTarget();

                target.FileName = logFile;
                target.ArchiveNumbering = ArchiveNumberingMode.Rolling;
                target.ArchiveEvery = FileArchivePeriod.None;
                target.ArchiveAboveSize = 10485760;
                target.MaxArchiveFiles = 1;
                target.Layout = new NLog.Layouts.SimpleLayout("${longdate}|${level:uppercase=true}|${logger}|${message}${onexception:EXCEPTION OCCURRED:|${exception:format=tostring}}");

                AsyncTargetWrapper wrapper = new AsyncTargetWrapper(target, 5000, AsyncTargetWrapperOverflowAction.Discard);

                LogManager.Configuration = new NLog.Config.LoggingConfiguration();
                LogManager.Configuration.AddTarget("file", wrapper);

                LoggingRule fileRule = new LoggingRule("*", LogLevel.Trace, wrapper);
                LogManager.Configuration.LoggingRules.Add(fileRule);

                LogManager.ReconfigExistingLoggers();

                configured = true;
            }
        }

        private void LogExceptionVerbose(string message, Exception exception)
        {
            if (exception.GetType() == typeof(AggregateException))
            {
                foreach (Exception iex in (exception as AggregateException).Flatten().InnerExceptions)
                {
                    this.LogExceptionVerbose(message, iex);
                }
            }
            else
            {
                Configure();                
                Log.ErrorException(message, exception);

                if (exception.InnerException != null)
                {
                    Log.ErrorException(message, exception.InnerException);
                }
            }
        }
    }
}
