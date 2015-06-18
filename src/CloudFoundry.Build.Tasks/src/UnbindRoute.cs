namespace CloudFoundry.Build.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using CloudFoundry.CloudController.V2.Client;
    using CloudFoundry.CloudController.V2.Client.Data;
    using Microsoft.Build.Framework;

    public class UnbindRoute : BaseTask
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
                    PagedResponseCollection<ListAllDomainsDeprecatedResponse> domainInfoList = client.DomainsDeprecated.ListAllDomainsDeprecated().Result;

                    foreach (string domain in app.Domains)
                    {
                        foreach (string host in app.Hosts)
                        {
                            Logger.LogMessage("Unbinding route {0}.{1} from app {2}", host, domain, app.Name);

                            ListAllDomainsDeprecatedResponse domainInfo = domainInfoList.Where(o => o.Name == domain).FirstOrDefault();
                            var routeGuid = Utils.GetRouteGuid(client, host, domainInfo.EntityMetadata.Guid.ToGuid());
                            if (routeGuid.HasValue)
                            {
                                client.Routes.RemoveAppFromRoute(routeGuid.Value, appGuid.Value);
                            }
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
                this.Logger.LogError("Unbind Route failed", exception);
                return false;
            }
        
            return true;
        }
    }
}
