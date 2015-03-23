using CloudFoundry.CloudController.V2;
using CloudFoundry.CloudController.V2.Client.Data;
using Microsoft.Build.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudFoundry.Build.Tasks
{
    public class CreateServices : BaseTask
    {
        [Required]
        public string Services { get; set; }

        [Required]
        public string Space { get; set; }

        [Output]
        public string[] ServicesGuids { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public override bool Execute()
        {

            logger = new Microsoft.Build.Utilities.TaskLoggingHelper(this);

            CloudFoundryClient client = InitClient();

            Guid? spaceGuid = null;

            if (Space.Length > 0)
            {
                PagedResponseCollection<ListAllSpacesResponse> spaceList = client.Spaces.ListAllSpaces(new RequestOptions() { Query = "name:" + Space }).Result;

                spaceGuid = new Guid(spaceList.FirstOrDefault().EntityMetadata.Guid);
            
                if (spaceGuid == null)
                {
                    logger.LogError("Space {0} not found", Space);
                    return false;
                }
            }

            List<ProvisionedService> servicesList = Utils.Deserialize<List<ProvisionedService>>(Services);

            List<string> serviceGuids = new List<string>();

            foreach (ProvisionedService service in servicesList)
            {
                logger.LogMessage("Creating {0} service {1}", service.Type, service.Name);
                Guid? planGuid = null;
                PagedResponseCollection<ListAllServicesResponse> allServicesList = client.Services.ListAllServices(new RequestOptions() { Query = "label:" + service.Type }).Result;

                foreach (var serviceInfo in allServicesList)
                {
                    var planList = client.Services.ListAllServicePlansForService(new Guid(serviceInfo.EntityMetadata.Guid)).Result;

                    var plan = planList.Where(o => o.Name == service.Plan).FirstOrDefault();

                    if (plan != null)
                    {
                        planGuid = new Guid(plan.EntityMetadata.Guid);
                        break;
                    }
                }

                CreateServiceInstanceRequest request = new CreateServiceInstanceRequest();

                request.Name = service.Name;
                request.ServicePlanGuid = planGuid;
                request.SpaceGuid = spaceGuid;

                CreateServiceInstanceResponse result = client.ServiceInstances.CreateServiceInstance(request).Result;

                serviceGuids.Add(result.EntityMetadata.Guid);
            }

            ServicesGuids = serviceGuids.ToArray();

            return true;
        }
    }
}
