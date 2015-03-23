using CloudFoundry.CloudController.V2;
using CloudFoundry.UAA;
using Microsoft.Build.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudFoundry.Build.Tasks
{
    public class BaseTask : ITask
    {
        internal Microsoft.Build.Utilities.TaskLoggingHelper logger;

        public IBuildEngine BuildEngine
        {
            get;
            set;
        }

        public ITaskHost HostObject
        {
            get;
            set;
        }

        [Required]
        public string User { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string ServerUri { get; set; }

        public virtual bool Execute()
        {
            return true;
        }

        internal CloudFoundryClient InitClient()
        {

            //skip ssl
            System.Net.ServicePointManager.ServerCertificateValidationCallback = ((sender, certificate, chain, sslPolicyErrors) => true);

            CloudCredentials creds = new CloudCredentials();
            creds.User = User;
            creds.Password = Password;

            CloudFoundryClient client = new CloudFoundryClient(new Uri(ServerUri), new System.Threading.CancellationToken());

            client.Login(creds).Wait();
            return client;
        }

    }
}
