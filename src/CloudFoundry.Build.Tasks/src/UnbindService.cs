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

            logger = new TaskLogger(this);
            try
            {
                CloudFoundryClient client = InitClient();

                logger.LogMessage("Unbinding service {0} from app {1}", CFServiceName, CFAppName);

                Guid? spaceGuid = null;

                if (CFSpace.Length > 0 && CFOrganization.Length > 0)
                {
                    spaceGuid = Utils.GetSpaceGuid(client, logger, CFOrganization, CFSpace);
                    if (spaceGuid == null)
                    {
                        return false;
                    }
                }


                var servicesList = client.Spaces.ListAllServiceInstancesForSpace(spaceGuid, new RequestOptions() { Query = "name:" + CFServiceName }).Result;

                if (servicesList.Count() > 1)
                {
                    logger.LogError("There are more services named {0} in space {1}", CFServiceName, CFSpace);
                    return false;
                }

                Guid serviceGuid = new Guid(servicesList.FirstOrDefault().EntityMetadata.Guid);

                PagedResponseCollection<ListAllAppsForSpaceResponse> appList = client.Spaces.ListAllAppsForSpace(spaceGuid, new RequestOptions() { Query = "name:" + CFAppName }).Result;
                if (appList.Count() > 1)
                {
                    logger.LogError("There are more applications named {0} in space {1}", CFAppName, CFSpace);
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
                this.logger.LogError("Unbind Service failed", exception);
                return false;
            }

            return true;
        }
    }
}
