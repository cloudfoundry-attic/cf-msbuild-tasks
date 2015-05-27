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

    public class CreateService : BaseTask
    {
        [Required]
        public string CFOrganization { get; set; }

        [Required]
        public string CFSpace { get; set; }

        [Required]
        public string CFServiceName { get; set; }

        [Required]
        public string CFServicePlan { get; set; }

        [Required]
        public string CFServiceType { get; set; }

        [Output]
        public string CFServiceGuid { get; set; }

        public override bool Execute()
        {
            this.Logger = new TaskLogger(this);

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

                Guid? planGuid = null;
                PagedResponseCollection<ListAllServicesResponse> servicesList = client.Services.ListAllServices(new RequestOptions() { Query = "label:" + this.CFServiceType }).Result;

                foreach (var service in servicesList)
                {
                    var planList = client.Services.ListAllServicePlansForService(new Guid(service.EntityMetadata.Guid)).Result;

                    var plan = planList.Where(o => o.Name == this.CFServicePlan).FirstOrDefault();

                    if (plan != null)
                    {
                        planGuid = new Guid(plan.EntityMetadata.Guid);
                        break;
                    }
                }

                CreateServiceInstanceRequest request = new CreateServiceInstanceRequest();

                request.Name = this.CFServiceName;
                request.ServicePlanGuid = planGuid;
                request.SpaceGuid = spaceGuid;

                CreateServiceInstanceResponse result = client.ServiceInstances.CreateServiceInstance(request).Result;

                this.CFServiceGuid = result.EntityMetadata.Guid;

                Logger.LogMessage("Created {0} service {1}", this.CFServiceType, result.Name);
            }
            catch (Exception exception)
            {
                this.Logger.LogError("Create Service failed", exception);
                return false;
            }

            return true;
        }
    }
}
