using CloudFoundry.CloudController.V2.Client;
using CloudFoundry.CloudController.V2.Client.Data;
using CloudFoundry.UAA;
using Microsoft.Build.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudFoundry.Build.Tasks
{
    public class UpdateApp : BaseTask
    {
        public string CFAppName { get; set; }

        public int CFAppMemory { get; set; }

        public int CFAppInstances { get; set; }

        public string CFAppBuildpack { get; set; }

        public string CFAppState { get; set; }

        [Required]
        public string CFAppGuid { get; set; }

        public override bool Execute()
        {
            logger = new Microsoft.Build.Utilities.TaskLoggingHelper(this);

            if (CFAppGuid.Length == 0)
            {
                logger.LogError("Application guid must be specified");
                return false;
            }

            CloudFoundryClient client = InitClient();

            UpdateAppRequest request = new UpdateAppRequest();

            request.Name = CFAppName;
            request.Memory = CFAppMemory;
            request.Instances = CFAppInstances;
            request.Buildpack = CFAppBuildpack;
            request.State = CFAppState;

            UpdateAppResponse response = client.Apps.UpdateApp(new Guid(CFAppGuid), request).Result;

            logger.LogMessage("Updated app {0} with guid {1}", response.Name, CFAppGuid);


            return true;
        }
    }
}
