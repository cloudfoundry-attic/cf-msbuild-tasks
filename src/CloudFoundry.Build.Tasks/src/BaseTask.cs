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
        internal TaskLogger logger;

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

        public bool CFSkipSslValidation { get; set; }

        [Required]
        public string CFServerUri { get; set; }

        public virtual bool Execute()
        {
            return true;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        internal CloudFoundryClient InitClient()
        {
            CloudFoundryClient client = new CloudFoundryClient(new Uri(CFServerUri), new System.Threading.CancellationToken(), null, CFSkipSslValidation);
            
            if (string.IsNullOrWhiteSpace(CFUser)==false && (string.IsNullOrWhiteSpace(CFPassword) == false || CFSavedPassword))
            {
                if (string.IsNullOrWhiteSpace(CFPassword))
                {
                    this.CFPassword = CloudCredentialsManager.GetPassword(new Uri(this.CFServerUri), this.CFUser);

                    if (string.IsNullOrWhiteSpace(this.CFPassword))
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
                var authContext =  client.Login(creds).Result;
                if (this is LoginTask)
                {
                    if (authContext.Token != null)
                    {
                        ((LoginTask)this).CFRefreshToken = authContext.Token.RefreshToken;
                    }

                }
            }
            else if (string.IsNullOrWhiteSpace(CFRefreshToken) == false)
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