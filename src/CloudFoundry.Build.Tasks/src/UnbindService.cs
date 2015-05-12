namespace CloudFoundry.Build.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using CloudFoundry.CloudController.V2.Client;
    using CloudFoundry.CloudController.V2.Client.Data;
    using Microsoft.Build.Framework;

    public class UnbindService : BaseTask
    {
        [Required]
        public string CFAppName { get; set; }

        [Required]
        public string CFServiceName { get; set; }

        [Required]
        public string CFOrganization { get; set; }

        [Required]
        public string CFSpace { get; set; }

        public override bool Execute()
        {
            this.Logger = new TaskLogger(this);
            try
            {
                CloudFoundryClient client = InitClient();

                Logger.LogMessage("Unbinding service {0} from app {1}", this.CFServiceName, this.CFAppName);

                Guid? spaceGuid = null;

                if ((!string.IsNullOrWhiteSpace(this.CFSpace)) && (!string.IsNullOrWhiteSpace(this.CFOrganization)))
                {
                    spaceGuid = Utils.GetSpaceGuid(client, this.Logger, this.CFOrganization, this.CFSpace);
                    if (spaceGuid == null)
                    {
                        return false;
                    }
                }

                var servicesList = client.Spaces.ListAllServiceInstancesForSpace(spaceGuid, new RequestOptions() { Query = "name:" + this.CFServiceName }).Result;

                if (servicesList.Count() > 1)
                {
                    Logger.LogError("There are more services named {0} in space {1}", this.CFServiceName, this.CFSpace);
                    return false;
                }

                Guid serviceGuid = new Guid(servicesList.FirstOrDefault().EntityMetadata.Guid);

                PagedResponseCollection<ListAllAppsForSpaceResponse> appList = client.Spaces.ListAllAppsForSpace(spaceGuid, new RequestOptions() { Query = "name:" + this.CFAppName }).Result;
                if (appList.Count() > 1)
                {
                    Logger.LogError("There are more applications named {0} in space {1}", this.CFAppName, this.CFSpace);
                    return false;
                }

                Guid appGuid = new Guid(appList.FirstOrDefault().EntityMetadata.Guid);

                var bindingsList = client.Apps.ListAllServiceBindingsForApp(appGuid).Result;

                foreach (var bind in bindingsList)
                {
                    if (bind.ServiceInstanceGuid.Value == serviceGuid)
                    {
                        client.Apps.RemoveServiceBindingFromApp(appGuid, new Guid(bind.EntityMetadata.Guid)).Wait();
                    }
                }
            }
            catch (Exception exception)
            {
                this.Logger.LogError("Unbind Service failed", exception);
                return false;
            }

            return true;
        }
    }
}
