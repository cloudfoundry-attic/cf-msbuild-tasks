﻿using CloudFoundry.CloudController.V2.Client;
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

        public string CFEnvironmentJson { get; set; }

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
                    request.EnvironmentJson = JsonConvert.DeserializeObject<Dictionary<string, string>>(CFEnvironmentJson);
                }

                UpdateAppResponse response = client.Apps.UpdateApp(new Guid(CFAppGuid), request).Result;

                logger.LogMessage("Updated app {0} with guid {1}", response.Name, CFAppGuid);
            }
            catch (AggregateException exception)
            {
                List<string> messages = new List<string>();
                ErrorFormatter.FormatExceptionMessage(exception, messages);
                this.logger.LogError(string.Join(Environment.NewLine, messages));
                return false;
            }

            return true;
        }
    }
}
