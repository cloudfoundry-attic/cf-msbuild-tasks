using CloudFoundry.CloudController.V2.Client;
using CloudFoundry.UAA;
using Microsoft.Build.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudFoundry.Build.Tasks
{
    public class PushApp : BaseTask
    {
        [Required]
        public string CFAppGuid { get; set; }

        [Required]
        public string CFAppPath { get; set; }

        public bool CFStart { get; set; }
        public override bool Execute()
        {
            
            logger = new TaskLogger(this);

            try
            {
                CloudFoundryClient client = InitClient();

                if (!Directory.Exists(CFAppPath))
                {
                    logger.LogError("Directory {0} not found", CFAppPath);
                    return false;
                }

                client.Apps.PushProgress += Apps_PushProgress;

                client.Apps.Push(new Guid(CFAppGuid), CFAppPath, CFStart).Wait();
            }
            catch (Exception exception)
            {
                this.logger.LogError("Push App failed", exception);
                return false;
            }
            return true;
        }

        void Apps_PushProgress(object sender, CloudController.V2.Client.PushProgressEventArgs e)
        {
            logger.LogMessage("Received from push job:{0} - {1}%", e.Message, e.Percent);
        }
    }
}
