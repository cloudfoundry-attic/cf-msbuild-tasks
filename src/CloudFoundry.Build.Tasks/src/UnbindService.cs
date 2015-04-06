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
        public string AppName { get; set; }

        [Required]
        public string ServiceName { get; set; }

        [Required]
        public string Space { get; set; }

        public override bool Execute()
        {

            logger = new Microsoft.Build.Utilities.TaskLoggingHelper(this);

            CloudFoundryClient client = InitClient();

            logger.LogMessage("Unbinding service {0} from app {1}", ServiceName, AppName);

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

            var servicesList = client.Spaces.ListAllServiceInstancesForSpace(spaceGuid, new RequestOptions() { Query = "name:" + ServiceName }).Result;

            if (servicesList.Count() > 1)
            {
                logger.LogError("There are more services named {0} in space {1}", ServiceName, Space);
                return false;
            }

            Guid serviceGuid=new Guid(servicesList.FirstOrDefault().EntityMetadata.Guid);

            PagedResponseCollection<ListAllAppsForSpaceResponse> appList = client.Spaces.ListAllAppsForSpace(spaceGuid, new RequestOptions() { Query = "name:" + AppName }).Result;
            if (appList.Count() > 1)
            {
                logger.LogError("There are more applications named {0} in space {1}", AppName, Space);
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
            

            return true;
        }
    }
}
