using CloudFoundry.CloudController.V2.Client;
using CloudFoundry.CloudController.V2.Client;
using CloudFoundry.UAA;
using Microsoft.Build.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly:CLSCompliant(true)]
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

        public string User { get; set; }

        public string Password { get; set; }

        public string RefreshToken { get; set; }

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

            CloudFoundryClient client = new CloudFoundryClient(new Uri(ServerUri), new System.Threading.CancellationToken());

            if (User.Length > 0 && Password.Length > 0)
            {
                CloudCredentials creds = new CloudCredentials();
                creds.User = User;
                creds.Password = Password;
                client.Login(creds).Wait();
            }
            else if (RefreshToken.Length > 0)
            {
                client.Login(RefreshToken).Wait();
            }
            else
            {
                throw new System.Security.Authentication.AuthenticationException("Could not authenticate client without refresh token or credentials!");
            }

            return client;
        }

    }
}
