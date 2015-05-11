using CloudFoundry.CloudController.Common.Exceptions;
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
    public class DeleteApp : BaseTask
    {
        [Required]
        public string CFAppName { get; set; }

        [Required]
        public string CFOrganization { get; set; }

        [Required]
        public string CFSpace { get; set; }

        public bool CFDeleteRoutes { get; set; }

        public bool CFDeleteServices { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public override bool Execute()
        {            
            logger = new Microsoft.Build.Utilities.TaskLoggingHelper(this);

            try
            {
                CloudFoundryClient client = InitClient();

                logger.LogMessage("Deleting application {0} from space {1}", CFAppName, CFSpace);

                Guid? spaceGuid = null;

                if (CFSpace.Length > 0 && CFOrganization.Length > 0)
                {
                    spaceGuid = Utils.GetSpaceGuid(client, logger, CFOrganization, CFSpace);
                    if (spaceGuid == null)
                    {
                        return false;
                    }
                }

                PagedResponseCollection<ListAllAppsForSpaceResponse> appList = client.Spaces.ListAllAppsForSpace(spaceGuid, new RequestOptions() { Query = "name:" + CFAppName }).Result;
                if (appList.Count() > 1)
                {
                    logger.LogError("There are more applications named {0} in space {1}", CFAppName, CFSpace);
                    return false;
                }

                Guid appGuid = new Guid(appList.FirstOrDefault().EntityMetadata.Guid);

                if (CFDeleteRoutes == true)
                {
                    logger.LogMessage("Deleting routes associated with {0}", CFAppName);
                    var routeList = client.Apps.ListAllRoutesForApp(appGuid).Result;
                    foreach (var route in routeList)
                    {
                        client.Routes.DeleteRoute(new Guid(route.EntityMetadata.Guid)).Wait();
                    }
                }

                if (CFDeleteServices == true)
                {
                    logger.LogMessage("Deleting services bound to {0}", CFAppName);

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
