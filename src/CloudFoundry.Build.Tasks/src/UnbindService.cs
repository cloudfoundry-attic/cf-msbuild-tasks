namespace CloudFoundry.Build.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using CloudFoundry.CloudController.V2.Client;
    using CloudFoundry.CloudController.V2.Client.Data;
    using Microsoft.Build.Framework;

    public class UnbindService : BaseTask
    {
        [Required]
        public string CFOrganization { get; set; }

        [Required]
        public string CFSpace { get; set; }

        public override bool Execute()
        {
            this.CFOrganization = this.CFOrganization.Trim();
            this.CFSpace = this.CFSpace.Trim();

            this.Logger = new TaskLogger(this);

            var app = LoadAppFromManifest();

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

                var appGuid = Utils.GetAppGuid(client, app.Name, spaceGuid.Value);

                if (appGuid.HasValue)
                {
                    foreach (string service in app.Services)
                    {
                        Logger.LogMessage("Unbinding service {0} from app {1}", service, app.Name);

                        var serviceGuid = Utils.GetServiceGuid(client, service, spaceGuid.Value);
                        if (serviceGuid.HasValue)
                        {
                            var bindingsList = client.Apps.ListAllServiceBindingsForApp(appGuid).Result;

                            foreach (var bind in bindingsList)
                            {
                                if (bind.ServiceInstanceGuid.Value == serviceGuid.Value)
                                {
                                    client.Apps.RemoveServiceBindingFromApp(appGuid, new Guid(bind.EntityMetadata.Guid)).Wait();
                                }
                            }
                        }
                        else
                        {
                            Logger.LogError("Service {0} not found in space {1}", service, this.CFSpace);
                        }
                    }
                }
                else
                {
                    Logger.LogError("App {0} not found in space {1}", app.Name, this.CFSpace);
                    return false;
                }
            }
            catch (Exception exception)
            {
                this.Logger.LogError("Unbind Service failed", exception);
                return false;
            }

            return true;
        }
    }
}
