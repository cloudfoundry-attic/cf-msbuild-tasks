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

    public class DeleteService : BaseTask
    {
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

                Logger.LogMessage("Deleting service {0} from space {1}", this.CFServiceName, this.CFSpace);

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

                client.ServiceInstances.DeleteServiceInstance(new Guid(servicesList.First().EntityMetadata.Guid)).Wait();
            }
            catch (Exception exception)
            {
                this.Logger.LogError("Delete Service failed", exception);
                return false;
            }

            return true;
        }
    }
}
