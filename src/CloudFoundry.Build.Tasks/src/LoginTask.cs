using CloudFoundry.CloudController.V2.Client;
using CloudFoundry.UAA;
using Microsoft.Build.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudFoundry.Build.Tasks
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Login", Justification="Using login name preferred")]
    public class LoginTask : ITask
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
        public string CFUser { get; set; }
        [Required]
        public string CFPassword { get; set; }
        [Required]
        public string CFServerUri { get; set; }
        [Output]
        public string CFRefreshToken { get; set; }

        public bool Execute()
        {
            //skip ssl
            System.Net.ServicePointManager.ServerCertificateValidationCallback = ((sender, certificate, chain, sslPolicyErrors) => true);

            logger = new Microsoft.Build.Utilities.TaskLoggingHelper(this);

            CloudFoundryClient client = new CloudFoundryClient(new Uri(CFServerUri), new System.Threading.CancellationToken());

            CloudCredentials creds = new CloudCredentials();
            creds.User = CFUser;
            creds.Password = CFPassword;
            AuthenticationContext context = client.Login(creds).Result;

            if (context.Token != null)
            {
                CFRefreshToken = context.Token.RefreshToken;
                logger.LogMessage("Login success - Refresh token: {0}", CFRefreshToken);
            }
            else
            {
                logger.LogError("Login failed, please check parameters");
                return false;
            }
            return true;
        }

    }
}
