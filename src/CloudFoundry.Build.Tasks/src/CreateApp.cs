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
        public string CFOrganization { get; set; }

        [Required]
        public string CFSpace { get; set; }

        [Output]
        public string CFAppGuid { get; set; }

        public override bool Execute()
        {
            this.CFOrganization = this.CFOrganization.Trim();
            this.CFSpace = this.CFSpace.Trim();

            var app = LoadAppFromManifest();

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

                if (!string.IsNullOrWhiteSpace(app.StackName))
                {
                    PagedResponseCollection<ListAllStacksResponse> stackList = client.Stacks.ListAllStacks().Result;

                    var stackInfo = stackList.Where(o => o.Name == app.StackName).FirstOrDefault();

                    if (stackInfo == null)
                    {
                        Logger.LogError("Stack {0} not found", app.StackName);
                        return false;
                    }

                    stackGuid = new Guid(stackInfo.EntityMetadata.Guid);
                }

                if (stackGuid.HasValue && spaceGuid.HasValue)
                {
                    PagedResponseCollection<ListAllAppsForSpaceResponse> apps = client.Spaces.ListAllAppsForSpace(spaceGuid, new RequestOptions() { Query = "name:" + app.Name }).Result;

                    if (apps.Count() > 0)
                    {
                        this.CFAppGuid = apps.FirstOrDefault().EntityMetadata.Guid;

                        UpdateAppRequest request = new UpdateAppRequest();
                        request.SpaceGuid = spaceGuid;
                        request.StackGuid = stackGuid;

                        request.EnvironmentJson = app.EnvironmentVariables;
                       
                        request.Memory = (int)app.Memory;
                       
                        request.Instances = app.InstanceCount;
                       
                        request.Buildpack = app.BuildpackUrl;
                        
                        request.Command = app.Command;

                        if (app.DiskQuota.HasValue)
                        {
                            request.DiskQuota = app.DiskQuota.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
                        }

                        UpdateAppResponse response = client.Apps.UpdateApp(new Guid(this.CFAppGuid), request).Result;
                        Logger.LogMessage("Updated app {0} with guid {1}", response.Name, response.EntityMetadata.Guid);
                    }
                    else
                    {
                        CreateAppRequest request = new CreateAppRequest();
                        request.Name = app.Name;
                        request.SpaceGuid = spaceGuid;
                        request.StackGuid = stackGuid;

                        request.EnvironmentJson = app.EnvironmentVariables;

                        request.Memory = (int)app.Memory;

                        request.Instances = app.InstanceCount;

                        request.Buildpack = app.BuildpackUrl;

                        request.Command = app.Command;

                        if (app.DiskQuota.HasValue)
                        {
                            request.DiskQuota = Convert.ToInt32(app.DiskQuota.Value, System.Globalization.CultureInfo.InvariantCulture);
                        }

                        CreateAppResponse response = client.Apps.CreateApp(request).Result;
                        this.CFAppGuid = response.EntityMetadata.Guid;
                        Logger.LogMessage("Created app {0} with guid {1}", app.Name, this.CFAppGuid);
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
