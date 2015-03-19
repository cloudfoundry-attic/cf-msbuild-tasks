using CloudFoundry.CloudController.V2;
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
        public string AppGuid { get; set; }

        [Required]
        public string AppPath { get; set; }

        public bool Start { get; set; }
        public override bool Execute()
        {
            
            logger = new Microsoft.Build.Utilities.TaskLoggingHelper(this);

            CloudFoundryClient client = InitClient();

            if (!Directory.Exists(AppPath))
            {
                logger.LogError("Directory {0} not found", AppPath);
                return false;
            }

            client.Apps.PushProgress += Apps_PushProgress;
            
            client.Apps.Push(new Guid(AppGuid), AppPath, Start).Wait();
            return true;
        }

        void Apps_PushProgress(object sender, CloudController.V2.Client.PushProgressEventArgs e)
        {
            logger.LogMessage("Received from push job:{0} - {1}%", e.Message, e.Percent);
        }
    }
}
