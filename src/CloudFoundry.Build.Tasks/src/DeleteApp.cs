namespace CloudFoundry.Build.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using CloudFoundry.CloudController.Common.Exceptions;
    using CloudFoundry.CloudController.V2.Client;
    using CloudFoundry.CloudController.V2.Client.Data;
    using Microsoft.Build.Framework;

    public class DeleteApp : BaseTask
    { 
        [Required]
        public string CFOrganization { get; set; }

        [Required]
        public string CFSpace { get; set; }

        public bool CFDeleteRoutes { get; set; }

        public bool CFDeleteServices { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Coupling needed")]
        public override bool Execute()
        {
            this.CFOrganization = this.CFOrganization.Trim();
            this.CFSpace = this.CFSpace.Trim();

            this.Logger = new TaskLogger(this);

            var app = LoadAppFromManifest();

            try
            {
                CloudFoundryClient client = InitClient();

                Logger.LogMessage("Deleting application {0} from space {1}", app.Name, this.CFSpace);

                Guid? spaceGuid = null;

                if ((!string.IsNullOrWhiteSpace(this.CFSpace)) && (!string.IsNullOrWhiteSpace(this.CFOrganization)))
                {
                    spaceGuid = Utils.GetSpaceGuid(client, this.Logger, this.CFOrganization, this.CFSpace);
                    if (spaceGuid == null)
                    {
                        return false;
                    }
                }

                PagedResponseCollection<ListAllAppsForSpaceResponse> appList = client.Spaces.ListAllAppsForSpace(spaceGuid, new RequestOptions() { Query = "name:" + app.Name }).Result;
                if (appList.Count() > 1)
                {
                    Logger.LogError("There are more applications named {0} in space {1}", app.Name, this.CFSpace);
                    return false;
                }

                Guid appGuid = new Guid(appList.FirstOrDefault().EntityMetadata.Guid);

                if (this.CFDeleteRoutes == true)
                {
                    Logger.LogMessage("Deleting routes associated with {0}", app.Name);
                    var routeList = client.Apps.ListAllRoutesForApp(appGuid).Result;
                    foreach (var route in routeList)
                    {
                        client.Routes.DeleteRoute(new Guid(route.EntityMetadata.Guid)).Wait();
                    }
                }

                if (this.CFDeleteServices == true)
                {
                    Logger.LogMessage("Deleting services bound to {0}", app.Name);

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
                                    Logger.LogWarning(e.Message);
                                }
                                else
                                {
                                    this.Logger.LogError("Delete App failed", ex);
                                    return false;
                                }
                            }
                        }
                    }
                }

                client.Apps.DeleteApp(appGuid).Wait();
            }
            catch (Exception exception)
            {
                this.Logger.LogError("Delete App failed", exception);
                return false;
            }

            return true;
        }
    }
}
