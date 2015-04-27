using CloudFoundry.CloudController.V2.Client;
using CloudFoundry.UAA;
using Microsoft.Build.Framework;
using System;
using System.Globalization;
using System.Security.Authentication;

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

        public string CFUser { get; set; }

        public string CFPassword { get; set; }

        public bool CFSavedPassword { get; set; }

        public string CFRefreshToken { get; set; }

        [Required]
        public string CFServerUri { get; set; }

        public virtual bool Execute()
        {
            return true;
        }

        internal CloudFoundryClient InitClient()
        {
            //skip ssl
            System.Net.ServicePointManager.ServerCertificateValidationCallback = ((sender, certificate, chain, sslPolicyErrors) => true);

            CloudFoundryClient client = new CloudFoundryClient(new Uri(CFServerUri), new System.Threading.CancellationToken());

            if (CFUser != null && (CFPassword != null || CFSavedPassword))
            {
                if (CFPassword == null)
                {
                    this.CFPassword = CloudCredentialsManager.GetPassword(new Uri(this.CFServerUri), this.CFUser);

                    if (this.CFPassword == null)
                    {
                        throw new AuthenticationException(
                            string.Format(CultureInfo.InvariantCulture,
                            "Could not find a password for user '{0}' and target '{1}' in your local credentials store. Either make sure the entry exists in your credentials store, or provide CFPassword.",
                            this.CFUser,
                            this.CFServerUri));
                    }
                }

                CloudCredentials creds = new CloudCredentials();
                creds.User = CFUser;
                creds.Password = CFPassword;
                client.Login(creds).Wait();
            }
            else if (CFRefreshToken != null)
            {
                client.Login(CFRefreshToken).Wait();
            }
            else
            {
                throw new AuthenticationException("Could not authenticate client without refresh token or credentials.");
            }

            return client;
        }
    }
}