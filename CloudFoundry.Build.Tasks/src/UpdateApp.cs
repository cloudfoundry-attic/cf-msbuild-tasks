using CloudFoundry.CloudController.V2;
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
        public string Name { get; set; }

        public int Memory { get; set; }

        public int Instances { get; set; }

        public string Buildpack { get; set; }

        public string State { get; set; }

        [Required]
        public string AppGuid { get; set; }

        public override bool Execute()
        {
            logger = new Microsoft.Build.Utilities.TaskLoggingHelper(this);

            if (AppGuid == string.Empty)
            {
                logger.LogError("Application guid must be specified");
                return false;
            }

            CloudFoundryClient client = InitClient();

            UpdateAppRequest request = new UpdateAppRequest();
            if (Name != string.Empty)
            {
                request.Name = Name;
            }
            if (Memory > 0)
            {
                request.Memory = Memory;
            }
            if (Instances > 0)
            {
                request.Instances = Instances;
            }
            if (Buildpack != string.Empty)
            {
                request.Buildpack = Buildpack;
            }
            if (State != string.Empty)
            {
                request.State = State;
            }

            UpdateAppResponse response = client.Apps.UpdateApp(new Guid(AppGuid), request).Result;

            logger.LogMessage("Updated app {0} with guid {1}", response.Name, AppGuid);


            return true;
        }
    }
}
