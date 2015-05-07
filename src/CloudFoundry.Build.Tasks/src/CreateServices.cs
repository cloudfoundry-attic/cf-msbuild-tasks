using CloudFoundry.CloudController.V2.Client;
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
        public string CFOrganization { get; set; }

        [Required]
        public string CFServices { get; set; }

        [Required]
        public string CFSpace { get; set; }

        [Output]
        public string[] CFServicesGuids { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public override bool Execute()
        {

            logger = new Microsoft.Build.Utilities.TaskLoggingHelper(this);

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
            List<ProvisionedService> servicesList = new List<ProvisionedService>();
            try
            {
               string[] provServs = CFServices.Split(';');

               foreach (string service in provServs)
               {
                   if (string.IsNullOrWhiteSpace(service) == false)
                   {
                       string[] serviceInfo = service.Split(',');

                       if (serviceInfo.Length != 3)
                       {
                           logger.LogError("Invalid service information in {0}", service);
                           continue;
                       }

                       ProvisionedService serviceDetails = new ProvisionedService();

                       serviceDetails.Name = serviceInfo[0].Trim();
                       serviceDetails.Type = serviceInfo[1].Trim();
                       serviceDetails.Plan = serviceInfo[2].Trim();

                       servicesList.Add(serviceDetails);
                   }
               }
            }
            catch(Exception ex)
            {
                logger.LogErrorFromException(ex);
                logger.LogWarning("Error trying to obtain service information, trying to deserialize as xml");
                servicesList = Utils.Deserialize<List<ProvisionedService>>(CFServices);
            }

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
                Guid? serviceInstanceGuid=null;
                if ((serviceInstanceGuid=Utils.CheckForExistingService(service.Name, planGuid, client)) != null)
                {
                    logger.LogMessage("Service {0} - {1} already exists -> skipping", service.Name, service.Type);
                    serviceGuids.Add(serviceInstanceGuid.Value.ToString());
                    continue;
                }

                CreateServiceInstanceRequest request = new CreateServiceInstanceRequest();

                request.Name = service.Name;
                request.ServicePlanGuid = planGuid;
                request.SpaceGuid = spaceGuid;

                CreateServiceInstanceResponse result = client.ServiceInstances.CreateServiceInstance(request).Result;

                serviceGuids.Add(result.EntityMetadata.Guid);
            }

            CFServicesGuids = serviceGuids.ToArray();

            return true;
        }
    }
}
