namespace CloudFoundry.Build.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using CloudFoundry.CloudController.V2.Client;
    using CloudFoundry.UAA;
    using Microsoft.Build.Framework;

    public class PushApp : BaseTask
    {
        [Required]
        public string CFAppGuid { get; set; }

        [Required]
        public string CFAppPath { get; set; }

        public bool CFStart { get; set; }

        public override bool Execute()
        {
            this.Logger = new TaskLogger(this);

            try
            {
                CloudFoundryClient client = InitClient();

                if (!Directory.Exists(this.CFAppPath))
                {
                    Logger.LogError("Directory {0} not found", this.CFAppPath);
                    return false;
                }

                client.Apps.PushProgress += this.Apps_PushProgress;

                client.Apps.Push(new Guid(this.CFAppGuid), this.CFAppPath, this.CFStart).Wait();
            }
            catch (Exception exception)
            {
                this.Logger.LogError("Push App failed", exception);
                return false;
            }

            return true;
        }

        private void Apps_PushProgress(object sender, CloudController.V2.Client.PushProgressEventArgs e)
        {
            Logger.LogMessage("Received from push job:{0} - {1}%", e.Message, e.Percent);
        }
    }
}
