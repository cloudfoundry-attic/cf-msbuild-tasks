namespace CloudFoundry.Build.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using CloudFoundry.CloudController.V2.Client;
    using CloudFoundry.CloudController.V2.Client.Data;
    using CloudFoundry.UAA;
    using Microsoft.Build.Framework;
    using Newtonsoft.Json;

    public class UpdateApp : BaseTask
    {
        public string CFAppName { get; set; }

        public int CFAppMemory { get; set; }

        public int CFAppInstances { get; set; }

        public string CFAppBuildpack { get; set; }

        public string CFAppState { get; set; }

        public string CFEnvironmentJson { get; set; }

        [Required]
        public string CFAppGuid { get; set; }

        public override bool Execute()
        {
            this.Logger = new TaskLogger(this);

        if (string.IsNullOrWhiteSpace(this.CFAppGuid))
            {
                Logger.LogError("Application guid must be specified");
                return false;
            }

            try
            {
                CloudFoundryClient client = InitClient();

                UpdateAppRequest request = new UpdateAppRequest();

                request.Name = this.CFAppName;
                request.Memory = this.CFAppMemory;
                request.Instances = this.CFAppInstances;
                request.Buildpack = this.CFAppBuildpack;
                request.State = this.CFAppState;

                if (this.CFEnvironmentJson != null)
                {
                    request.EnvironmentJson = JsonConvert.DeserializeObject<Dictionary<string, string>>(this.CFEnvironmentJson);
                }

                UpdateAppResponse response = client.Apps.UpdateApp(new Guid(this.CFAppGuid), request).Result;

                Logger.LogMessage("Updated app {0} with guid {1}", response.Name, this.CFAppGuid);
            }
            catch (Exception exception)
            {
                this.Logger.LogError("Update App failed", exception);
                return false;
            }

            return true;
        }
    }
}
