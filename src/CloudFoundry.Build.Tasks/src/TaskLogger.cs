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

    internal class TaskLogger
    {
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
            }
        }

        internal void LogError(string message, params object[] args)
        {
            this.LogError(message, null, args);
        }

        internal void LogMessage(string message, params object[] args)
        {
            this.logger.LogMessage(message, args);
        }

        internal void LogWarning(string message, params object[] args)
        {
            this.logger.LogWarning(message, args);
        }

        internal void LogErrorFromException(Exception ex)
        {
            this.logger.LogErrorFromException(ex);
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
                this.logger.LogMessage(MessageImportance.Low, string.Format(CultureInfo.InstalledUICulture, "{0}: {1}", message, exception.ToString()));
            }
        }
    }
}
