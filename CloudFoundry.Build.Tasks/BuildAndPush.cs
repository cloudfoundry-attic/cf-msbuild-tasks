using CloudFoundry.UAA;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudFoundry.Build.Tasks
{
    public class BuildAndPush : ITask
    {
        private IBuildEngine buildEngine;
        private ITaskHost taskHost;
        private string user;
        private string password;
        private string spaceName;
        private Uri serverUri;
        private string configurationPath;
        private TaskLoggingHelper logger;
        public IBuildEngine BuildEngine
        {
            get { return buildEngine; }
            set { buildEngine = value; }
        }

        public String User
        {
            get { return user; }
            set { user = value; }
        }

        public String Password
        {
            get { return password; }
            set { password = value; }
        }

        public String SpaceName
        {
            get { return spaceName; }
            set { spaceName = value; }
        }

        public Uri ServerUri
        {
            get { return serverUri; }
            set { serverUri = value; }
        }

        public string ConfigurationPath
        {
            get { return configurationPath; }
            set { configurationPath = value; }
        }

        public bool Execute()
        {
            bool ok = true;
            logger = new Microsoft.Build.Utilities.TaskLoggingHelper(this);

            try
            {
                CloudCredentials creds = new CloudCredentials();
                creds.User = User;
                creds.Password = Password;

                PushJob job = new PushJob(creds, new System.Threading.CancellationToken(), spaceName, ServerUri, ConfigurationPath);

                job.progressEvent -= job_progressEvent;
                job.progressEvent += job_progressEvent;
                job.Start().Wait();
            }
            catch (Exception ex)
            {
                ok = false;
                logger.LogErrorFromException(ex);
            }

            return ok;
        }

        void job_progressEvent(object sender, CloudController.V2.Client.PushProgressEventArgs e)
        {
            logger.LogMessage(MessageImportance.Normal, "{0} - {1}", e.Message, e.Percent);
        }

        public ITaskHost HostObject
        {
            get { return taskHost; }
            set { taskHost = value; }
        }
    }
}
