using CloudFoundry.CloudController.V2.Client;
using CloudFoundry.CloudController.V2.Client.Data;
using CloudFoundry.UAA;
using Microsoft.Build.Framework;
using Newtonsoft.Json;
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

        public ITaskItem[] CFEnvironmentJson { get; set; }

        [Required]
        public string CFAppGuid { get; set; }

        public override bool Execute()
        {
            logger = new TaskLogger(this);

            if (string.IsNullOrWhiteSpace(CFAppGuid))
            {
                logger.LogError("Application guid must be specified");
                return false;
            }

            try
            {
                CloudFoundryClient client = InitClient();

                UpdateAppRequest request = new UpdateAppRequest();

            request.Name = CFAppName;
            request.Memory = CFAppMemory;
            request.Instances = CFAppInstances;
            request.Buildpack = CFAppBuildpack;
            request.State = CFAppState;

            if (CFEnvironmentJson != null)
            {
                Dictionary<string, string> EnvDict = new Dictionary<string, string>();
                foreach (ITaskItem item in CFEnvironmentJson)
                {
                    if (Utils.IsJson(item.ToString()))
                    {
                        EnvDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(item.ToString());
                        break;
                    }
                    else
                    {
                        EnvDict.Add(item.ToString(), item.GetMetadata("Value"));

                if (CFEnvironmentJson != null)
                {
                    request.EnvironmentJson = JsonConvert.DeserializeObject<Dictionary<string, string>>(CFEnvironmentJson);
                }

                UpdateAppResponse response = client.Apps.UpdateApp(new Guid(CFAppGuid), request).Result;

                logger.LogMessage("Updated app {0} with guid {1}", response.Name, CFAppGuid);
            }
            catch (Exception exception)
            {
                this.logger.LogError("Update App failed", exception);
                return false;
            }

            return true;
        }
    }
}
