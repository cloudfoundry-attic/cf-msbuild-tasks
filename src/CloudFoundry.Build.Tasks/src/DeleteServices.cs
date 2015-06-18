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

    public class DeleteServices : BaseTask
    {
        [Required]
        public string CFOrganization { get; set; }

        [Required]
        public string CFSpace { get; set; }

        public override bool Execute()
        {
            this.CFOrganization = this.CFOrganization.Trim();
            this.CFSpace = this.CFSpace.Trim();

            this.Logger = new TaskLogger(this);

            var app = LoadAppFromManifest();
            try
            {
                CloudFoundryClient client = InitClient();

                Guid? spaceGuid = null;

                if ((!string.IsNullOrWhiteSpace(this.CFSpace)) && (!string.IsNullOrWhiteSpace(this.CFOrganization)))
                {
                    spaceGuid = Utils.GetSpaceGuid(client, this.Logger, this.CFOrganization, this.CFSpace);
                    if (spaceGuid == null)
                    {
                        return false;
                    }
                }

                foreach (string service in app.Services)
                {
                    Logger.LogMessage("Deleting service {0} from space {1}", service, this.CFSpace);

                    var serviceGuid = Utils.GetServiceGuid(client, service, spaceGuid.Value);

                    if (serviceGuid.HasValue)
                    {
                        client.ServiceInstances.DeleteServiceInstance(serviceGuid.Value).Wait();
                    }
                    else
                    {
                        Logger.LogError("Service {0} not found in space {1}", service, this.CFSpace);
                    }
                }
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
