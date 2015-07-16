namespace CloudFoundry.Build.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using CloudFoundry.CloudController.Common.Exceptions;
    using CloudFoundry.CloudController.V2.Client;
    using CloudFoundry.UAA;
    using Microsoft.Build.Framework;

    public class PushApp : BaseTask
    {
        [Required]
        public string CFOrganization { get; set; }

        [Required]
        public string CFSpace { get; set; }

        [Required]
        public string CFAppPath { get; set; }

        public override bool Execute()
        {
            this.CFOrganization = this.CFOrganization.Trim();
            this.CFSpace = this.CFSpace.Trim();
            this.CFAppPath = this.CFAppPath.Trim();

            this.Logger = new TaskLogger(this);
            var app = LoadAppFromManifest();

            try
            {
                CloudFoundryClient client = InitClient();

                Guid? spaceGuid = null;

                if ((!string.IsNullOrWhiteSpace(this.CFSpace)) && (!string.IsNullOrWhiteSpace(this.CFOrganization)))
                {
                    spaceGuid = Utils.GetSpaceGuid(client, this.Logger, this.CFOrganization, this.CFSpace);

                    if (spaceGuid == null)
                    {
                        return false;
                    }
                }

                var appGuid = Utils.GetAppGuid(client, app.Name, spaceGuid.Value);
                if (appGuid.HasValue)
                {
                    if (!Directory.Exists(this.CFAppPath))
                    {
                        Logger.LogError("Directory {0} not found", this.CFAppPath);
                        return false;
                    }

                    client.Apps.PushProgress += this.Apps_PushProgress;

                    client.Apps.Push(appGuid.Value, this.CFAppPath, false).Wait();
                }
                else
                {
                    Logger.LogError("App {0} not found ", app.Name);
                    return false;
                }
            }
            catch (AggregateException ex)
            {
                foreach (Exception e in ex.Flatten().InnerExceptions)
                {
                    if (e is CloudFoundryException)
                    {
                        Logger.LogWarning(e.Message);
                    }
                    else
                    {
                        this.Logger.LogError("Push job failed", ex);
                        return false;
                    }
                }
            }

            return true;
        }

        private void Apps_PushProgress(object sender, CloudController.V2.Client.PushProgressEventArgs e)
        {
            Logger.LogMessage("Received from push job:{0} - {1}%", e.Message, e.Percent);
        }
    }
}
