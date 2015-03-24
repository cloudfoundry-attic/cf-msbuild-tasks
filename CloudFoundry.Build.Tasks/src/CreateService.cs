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
        public string Space { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string ServicePlan { get; set; }

        [Required]
        public string ServiceType { get; set; }

        [Output]
        public string ServiceGuid { get; set; }

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

            Guid? planGuid = null;
            PagedResponseCollection<ListAllServicesResponse> servicesList = client.Services.ListAllServices(new RequestOptions() { Query = "label:" + ServiceType }).Result;

            foreach (var service in servicesList)
            {
               var planList = client.Services.ListAllServicePlansForService(new Guid(service.EntityMetadata.Guid)).Result;

               var plan = planList.Where(o => o.Name == ServicePlan).FirstOrDefault();

               if (plan != null)
               {
                   planGuid = new Guid(plan.EntityMetadata.Guid);
                   break;
               }
            }

            CreateServiceInstanceRequest request = new CreateServiceInstanceRequest();

            request.Name = Name;
            request.ServicePlanGuid = planGuid;
            request.SpaceGuid = spaceGuid;

            CreateServiceInstanceResponse result = client.ServiceInstances.CreateServiceInstance(request).Result;

            ServiceGuid = result.EntityMetadata.Guid;

            logger.LogMessage("Created {0} service {1}", ServiceType, result.Name);
            return true;
        }
    }
}
