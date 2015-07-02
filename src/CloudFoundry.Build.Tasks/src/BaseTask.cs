namespace CloudFoundry.Build.Tasks
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Security.Authentication;
    using System.Threading;
    using CloudFoundry.CloudController.V2.Client;
    using CloudFoundry.Manifests;
    using CloudFoundry.Manifests.Models;
    using CloudFoundry.UAA;
    using Microsoft.Build.Framework;
    
    public class BaseTask : ITask, ICancelableTask
    {
        private TaskLogger logger;
        
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
        public string CFManifest { get; set; }

        [Required]
        public string CFServerUri { get; set; }

        internal TaskLogger Logger
        {
            get { return this.logger; }
            set { this.logger = value; }
        }

        internal CancellationTokenSource CancelToken { get; set; }

        public virtual bool Execute()
        {
            return true;
        }

        public void Cancel()
        {
            this.CancelToken.Cancel();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily", Justification = "Cast needed to change output property")]
        internal CloudFoundryClient InitClient()
        {
            this.CFServerUri = this.CFServerUri.Trim();
            this.CancelToken = new CancellationTokenSource();

            CloudFoundryClient client = new CloudFoundryClient(new Uri(this.CFServerUri), this.CancelToken.Token, null, this.CFSkipSslValidation);

            if (string.IsNullOrWhiteSpace(this.CFUser) == false && (string.IsNullOrWhiteSpace(this.CFPassword) == false || this.CFSavedPassword))
            {
                this.CFUser = this.CFUser.Trim();

                if (string.IsNullOrWhiteSpace(this.CFPassword))
                {
                    this.CFPassword = CloudCredentialsManager.GetPassword(new Uri(this.CFServerUri), this.CFUser);

                    if (string.IsNullOrWhiteSpace(this.CFPassword))
                    {
                        throw new AuthenticationException(
                            string.Format(
                            CultureInfo.InvariantCulture,
                            "Could not find a password for user '{0}' and target '{1}' in your local credentials store. Either make sure the entry exists in your credentials store, or provide CFPassword.",
                            this.CFUser,
                            this.CFServerUri));
                    }
                }

                this.CFPassword = this.CFPassword;
         
                CloudCredentials creds = new CloudCredentials();
                creds.User = this.CFUser;
                creds.Password = this.CFPassword;
                var authContext = client.Login(creds).Result;
                if (this is LoginTask)
                {
                    if (authContext.Token != null)
                    {
                        ((LoginTask)this).CFRefreshToken = authContext.Token.RefreshToken;
                    }
                }
            }
            else if (string.IsNullOrWhiteSpace(this.CFRefreshToken) == false)
            {
                this.CFRefreshToken = this.CFRefreshToken.Trim();

                client.Login(this.CFRefreshToken).Wait();
            }
            else
            {
                throw new AuthenticationException("Could not authenticate client without refresh token or credentials.");
            }

            return client;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes", Justification = "Throw exception if more than one application present in manifest")]
        internal Application LoadAppFromManifest()
        {
            this.CFManifest = this.CFManifest.Trim();

            Manifest man = ManifestDiskRepository.ReadManifest(this.CFManifest);
         
            if (man.Applications().Length > 1)
            {
                throw new InvalidOperationException("More than one application specified in the manifest file");
            }

            return man.Applications()[0];
        }
    }
}