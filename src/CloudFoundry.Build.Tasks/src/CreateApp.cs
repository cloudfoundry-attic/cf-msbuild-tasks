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
    public class CreateApp : BaseTask
    {
        [Required]
        public string CFAppName { get; set;}

        [Required]
        public string CFOrganization { get; set; }

        [Required]
        public string CFSpace { get; set; }

        [Required]
        public string CFStack { get; set; }

        public int CFAppMemory { get; set; }

        public int CFAppInstances { get; set; }

        public string CFEnvironmentJson { get; set; }

        public string CFAppBuildpack { get; set; }

        [Output]
        public string CFAppGuid { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public override bool Execute()
        {
            logger = new TaskLogger(this);
            CloudFoundryClient client = InitClient();

            Guid? spaceGuid = null;
            Guid? stackGuid = null;

            try
            {
                if ((!string.IsNullOrWhiteSpace(CFSpace)) && (!string.IsNullOrWhiteSpace(CFOrganization)))
                {
                    spaceGuid = Utils.GetSpaceGuid(client, logger, CFOrganization, CFSpace);
                    if (spaceGuid == null)
                    {
                        return false;
                    }
                }

                if (!string.IsNullOrWhiteSpace(CFStack))
                {
                    PagedResponseCollection<ListAllStacksResponse> stackList = client.Stacks.ListAllStacks().Result;

                    var stackInfo = stackList.Where(o => o.Name == CFStack).FirstOrDefault();

                    if (stackInfo == null)
                    {
                        logger.LogError("Stack {0} not found", CFStack);
                        return false;
                    }
                    stackGuid = new Guid(stackInfo.EntityMetadata.Guid);
                }

                if (stackGuid.HasValue && spaceGuid.HasValue)
                {
                    PagedResponseCollection<ListAllAppsForSpaceResponse> apps = client.Spaces.ListAllAppsForSpace(spaceGuid, new RequestOptions() { Query = "name:" + CFAppName }).Result;

                    if (apps.Count() > 0)
                    {
                        CFAppGuid = apps.FirstOrDefault().EntityMetadata.Guid;

                        UpdateAppRequest request = new UpdateAppRequest();
                        request.SpaceGuid = spaceGuid;
                        request.StackGuid = stackGuid;

                        if (CFEnvironmentJson != null)
                        {
                            request.EnvironmentJson = JsonConvert.DeserializeObject<Dictionary<string, string>>(CFEnvironmentJson);
                        }

                        if (CFAppMemory > 0)
                        {
                            request.Memory = CFAppMemory;
                        }
                        if (CFAppInstances > 0)
                        {
                            request.Instances = CFAppInstances;
                        }
                        if (CFAppBuildpack != null)
                        {
                            request.Buildpack = CFAppBuildpack;
                        }

                        UpdateAppResponse response = client.Apps.UpdateApp(new Guid(CFAppGuid), request).Result;
                        logger.LogMessage("Updated app {0} with guid {1}", response.Name, response.EntityMetadata.Guid);
                    }
                    else
                    {

                        CreateAppRequest request = new CreateAppRequest();
                        request.Name = CFAppName;
                        request.SpaceGuid = spaceGuid;
                        request.StackGuid = stackGuid;

                        if (CFEnvironmentJson != null)
                        {
                            request.EnvironmentJson = JsonConvert.DeserializeObject<Dictionary<string, string>>(CFEnvironmentJson);
                        }

                        if (CFAppMemory > 0)
                        {
                            request.Memory = CFAppMemory;
                        }
                        if (CFAppInstances > 0)
                        {
                            request.Instances = CFAppInstances;
                        }
                        if (CFAppBuildpack != null)
                        {
                            request.Buildpack = CFAppBuildpack;
                        }

                        CreateAppResponse response = client.Apps.CreateApp(request).Result;
                        CFAppGuid = response.EntityMetadata.Guid;
                        logger.LogMessage("Created app {0} with guid {1}", CFAppName, CFAppGuid);
                    }
                }
            }
            catch (Exception exception)
            {
                this.logger.LogError("Create App failed", exception);
                return false;
            }
            return true;
        }


    }
}
