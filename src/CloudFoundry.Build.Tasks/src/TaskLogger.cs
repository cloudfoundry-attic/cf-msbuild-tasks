using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System.Globalization;
using System.IO;
using NLog.Targets;
using NLog.Targets.Wrappers;
using NLog.Config;

namespace CloudFoundry.Build.Tasks
{
    internal class TaskLogger
    {
        private static readonly NLog.Logger nlog = LogManager.GetLogger(System.AppDomain.CurrentDomain.FriendlyName);
        private TaskLoggingHelper logger;
        private static bool configured = false;
        private static readonly object configureLock = new object();
        private static string logFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "cf-msbuild-tasks", "cf-msbuild-tasks.log");

        public TaskLogger(ITask task)
        {
            this.logger = new TaskLoggingHelper(task);
        }

        private static void Configure()
        {
            if (configured)
            {
                return;
            }

            lock (configureLock)
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

        internal void LogError(string message, Exception exception, params object[] args)
        {
            if(exception != null)
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
                nlog.Error(CultureInfo.InvariantCulture, message, args);
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
            nlog.Info(CultureInfo.InvariantCulture, message, args);
        }

        private void LogExceptionVerbose(string message, Exception exception)
        {
            if (exception is AggregateException)
            {
                foreach (Exception iex in (exception as AggregateException).Flatten().InnerExceptions)
                {
                    LogExceptionVerbose(message, iex);
                }
            }
            else
            {
                Configure();                
                nlog.ErrorException(message, exception);

                if (exception.InnerException != null)
                {
                    nlog.ErrorException(message, exception.InnerException);
                }
            }
        }

        internal void LogWarning(string message, params object[] args)
        {
            this.logger.LogWarning(message, args);
            Configure();
            nlog.Warn(CultureInfo.InvariantCulture, message, args);
        }

        internal void LogErrorFromException(Exception ex)
        {
            this.logger.LogErrorFromException(ex);
            Configure();
            nlog.ErrorException(ex.Message, ex);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        private static void FormatExceptionMessage(Exception ex, List<string> message)
        {
            if (ex is AggregateException)
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
    }
}
