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

    public class CreateApp : BaseTask
    {
        [Required]
        public string CFAppName { get; set; }

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

        public override bool Execute()
        {
            this.Logger = new TaskLogger(this);
            CloudFoundryClient client = InitClient();

            Guid? spaceGuid = null;
            Guid? stackGuid = null;

            try
            {
                if ((!string.IsNullOrWhiteSpace(this.CFSpace)) && (!string.IsNullOrWhiteSpace(this.CFOrganization)))
                {
                    spaceGuid = Utils.GetSpaceGuid(client, this.Logger, this.CFOrganization, this.CFSpace);
                    if (spaceGuid == null)
                    {
                        return false;
                    }
                }

                if (!string.IsNullOrWhiteSpace(this.CFStack))
                {
                    PagedResponseCollection<ListAllStacksResponse> stackList = client.Stacks.ListAllStacks().Result;

                    var stackInfo = stackList.Where(o => o.Name == this.CFStack).FirstOrDefault();

                    if (stackInfo == null)
                    {
                        Logger.LogError("Stack {0} not found", this.CFStack);
                        return false;
                    }

                    stackGuid = new Guid(stackInfo.EntityMetadata.Guid);
                }

                if (stackGuid.HasValue && spaceGuid.HasValue)
                {
                    PagedResponseCollection<ListAllAppsForSpaceResponse> apps = client.Spaces.ListAllAppsForSpace(spaceGuid, new RequestOptions() { Query = "name:" + this.CFAppName }).Result;

                    if (apps.Count() > 0)
                    {
                        this.CFAppGuid = apps.FirstOrDefault().EntityMetadata.Guid;

                        UpdateAppRequest request = new UpdateAppRequest();
                        request.SpaceGuid = spaceGuid;
                        request.StackGuid = stackGuid;

                        if (this.CFEnvironmentJson != null)
                        {
                            request.EnvironmentJson = JsonConvert.DeserializeObject<Dictionary<string, string>>(this.CFEnvironmentJson);
                        }

                        if (this.CFAppMemory > 0)
                        {
                            request.Memory = this.CFAppMemory;
                        }

                        if (this.CFAppInstances > 0)
                        {
                            request.Instances = this.CFAppInstances;
                        }
                        
                        if (this.CFAppBuildpack != null)
                        {
                            request.Buildpack = this.CFAppBuildpack;
                        }

                        UpdateAppResponse response = client.Apps.UpdateApp(new Guid(this.CFAppGuid), request).Result;
                        Logger.LogMessage("Updated app {0} with guid {1}", response.Name, response.EntityMetadata.Guid);
                    }
                    else
                    {
                        CreateAppRequest request = new CreateAppRequest();
                        request.Name = this.CFAppName;
                        request.SpaceGuid = spaceGuid;
                        request.StackGuid = stackGuid;

                        if (this.CFEnvironmentJson != null)
                        {
                            request.EnvironmentJson = JsonConvert.DeserializeObject<Dictionary<string, string>>(this.CFEnvironmentJson);
                        }

                        if (this.CFAppMemory > 0)
                        {
                            request.Memory = this.CFAppMemory;
                        }

                        if (this.CFAppInstances > 0)
                        {
                            request.Instances = this.CFAppInstances;
                        }
                        
                        if (this.CFAppBuildpack != null)
                        {
                            request.Buildpack = this.CFAppBuildpack;
                        }

                        CreateAppResponse response = client.Apps.CreateApp(request).Result;
                        this.CFAppGuid = response.EntityMetadata.Guid;
                        Logger.LogMessage("Created app {0} with guid {1}", this.CFAppName, this.CFAppGuid);
                    }
                }
            }
            catch (Exception exception)
            {
                this.Logger.LogError("Create App failed", exception);
                return false;
            }

            return true;
        }
    }
}
