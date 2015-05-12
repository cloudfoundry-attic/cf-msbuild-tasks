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
        public string CFRoute { get; set; }

        [Required]
        public string CFAppName { get; set; }

        [Required]
        public string CFOrganization { get; set; }

        [Required]
        public string CFSpace { get; set; }

        public override bool Execute()
        {
            this.Logger = new TaskLogger(this);

            try
            {
                CloudFoundryClient client = InitClient();

                Logger.LogMessage("Unbinding route {0} from app {1}", this.CFRoute, this.CFAppName);

                string domain = string.Empty;
                string host = string.Empty;
                Utils.ExtractDomainAndHost(this.CFRoute, out domain, out host);

                if (string.IsNullOrWhiteSpace(domain) || string.IsNullOrWhiteSpace(host))
                {
                    Logger.LogError("Error extracting domain and host information from route {0}", this.CFRoute);
                    return false;
                }

                PagedResponseCollection<ListAllDomainsDeprecatedResponse> domainInfoList = client.DomainsDeprecated.ListAllDomainsDeprecated().Result;
                ListAllDomainsDeprecatedResponse domainInfo = domainInfoList.Where(o => o.Name == domain).FirstOrDefault();

                Guid? spaceGuid = null;

                if ((!string.IsNullOrWhiteSpace(this.CFSpace)) && (!string.IsNullOrWhiteSpace(this.CFOrganization)))
                {
                    spaceGuid = Utils.GetSpaceGuid(client, this.Logger, this.CFOrganization, this.CFSpace);
                    if (spaceGuid == null)
                    {
                        return false;
                    }
                }

                PagedResponseCollection<ListAllAppsForSpaceResponse> appList = client.Spaces.ListAllAppsForSpace(spaceGuid, new RequestOptions() { Query = "name:" + this.CFAppName }).Result;
                if (appList.Count() > 1)
                {
                    Logger.LogError("There are more applications named {0} in space {1}", this.CFAppName, this.CFSpace);
                    return false;
                }

                Guid appGuid = new Guid(appList.FirstOrDefault().EntityMetadata.Guid);

                PagedResponseCollection<ListAllRoutesForAppResponse> routeList = client.Apps.ListAllRoutesForApp(appGuid).Result;

                ListAllRoutesForAppResponse routeInfo = routeList.Where(o => o.Host == host && o.DomainGuid == new Guid(domainInfo.EntityMetadata.Guid)).FirstOrDefault();

                if (routeInfo == null)
                {
                    Logger.LogError("Route {0} not found in {1}'s routes", this.CFRoute, this.CFAppName);
                    return false;
                }

                client.Routes.RemoveAppFromRoute(new Guid(routeInfo.EntityMetadata.Guid), appGuid).Wait();
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
