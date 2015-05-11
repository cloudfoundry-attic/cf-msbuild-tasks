using CloudFoundry.CloudController.V2.Client;
using CloudFoundry.CloudController.V2.Client.Data;
using CloudFoundry.UAA;
using Microsoft.Build.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudFoundry.Build.Tasks
{
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
           
            logger = new Microsoft.Build.Utilities.TaskLoggingHelper(this);

            try
            {
                CloudFoundryClient client = InitClient();

                Guid? spaceGuid = null;

                if (CFSpace.Length > 0 && CFOrganization.Length > 0)
                {
                    spaceGuid = Utils.GetSpaceGuid(client, logger, CFOrganization, CFSpace);

                    if (spaceGuid == null)
                    {
                        return false;
                    }
                }

                Guid? planGuid = null;
                PagedResponseCollection<ListAllServicesResponse> servicesList = client.Services.ListAllServices(new RequestOptions() { Query = "label:" + CFServiceType }).Result;

                foreach (var service in servicesList)
                {
                    var planList = client.Services.ListAllServicePlansForService(new Guid(service.EntityMetadata.Guid)).Result;

                    var plan = planList.Where(o => o.Name == CFServicePlan).FirstOrDefault();

                    if (plan != null)
                    {
                        planGuid = new Guid(plan.EntityMetadata.Guid);
                        break;
                    }
                }

                CreateServiceInstanceRequest request = new CreateServiceInstanceRequest();

                request.Name = CFServiceName;
                request.ServicePlanGuid = planGuid;
                request.SpaceGuid = spaceGuid;

                CreateServiceInstanceResponse result = client.ServiceInstances.CreateServiceInstance(request).Result;

                CFServiceGuid = result.EntityMetadata.Guid;

                logger.LogMessage("Created {0} service {1}", CFServiceType, result.Name);
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
