using CloudFoundry.CloudController.Common.Exceptions;
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
    public class DeleteApp : BaseTask
    {
        [Required]
        public string AppName { get; set; }

        [Required]
        public string Space { get; set; }

        public bool DeleteRoutes { get; set; }

        public bool DeleteServices { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public override bool Execute()
        {

            logger = new Microsoft.Build.Utilities.TaskLoggingHelper(this);

            CloudFoundryClient client = InitClient();

            logger.LogMessage("Deleting application {0} from space {1}", AppName, Space);

            PagedResponseCollection<ListAllSpacesResponse> spaceList = client.Spaces.ListAllSpaces(new RequestOptions() { Query = "name:" + Space }).Result;

            Guid? spaceGuid = null;
            spaceGuid= new Guid(spaceList.FirstOrDefault().EntityMetadata.Guid);

            if (spaceGuid == null)
            {
                logger.LogError("Space {0} not found", Space);
                return false;
            }

            PagedResponseCollection<ListAllAppsForSpaceResponse> appList = client.Spaces.ListAllAppsForSpace(spaceGuid, new RequestOptions() { Query = "name:" + AppName }).Result;
            if (appList.Count() > 1)
            {
                logger.LogError("There are more applications named {0} in space {1}", AppName, Space);
                return false;
            }

            Guid appGuid = new Guid(appList.FirstOrDefault().EntityMetadata.Guid);

            if (DeleteRoutes == true)
            {
                logger.LogMessage("Deleting routes associated with {0}", AppName);
                var routeList = client.Apps.ListAllRoutesForApp(appGuid).Result;
                foreach (var route in routeList)
                {
                    client.Routes.DeleteRoute(new Guid(route.EntityMetadata.Guid)).Wait();
                }
            }

            if (DeleteServices == true)
            {
                logger.LogMessage("Deleting services bound to {0}", AppName);
             
                var serviceBindingList = client.Apps.ListAllServiceBindingsForApp(appGuid).Result;

                foreach (var serviceBind in serviceBindingList)
                {
                    client.ServiceBindings.DeleteServiceBinding(new Guid(serviceBind.EntityMetadata.Guid)).Wait();
                    try
                    {
                        client.ServiceInstances.DeleteServiceInstance(serviceBind.ServiceInstanceGuid).Wait();
                    }
                    catch (AggregateException ex)
                    {
                        foreach (Exception e in ex.Flatten().InnerExceptions)
                        {
                            if (e is CloudFoundryException)
                            {
                                logger.LogWarning(e.Message);
                            }
                            else
                            {
                                throw;
                            }
                        }
                    }
                }
            }

            client.Apps.DeleteApp(appGuid).Wait();

            return true;
        }
    }
}
