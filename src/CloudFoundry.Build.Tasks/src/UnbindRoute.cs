using CloudFoundry.CloudController.V2.Client;
using CloudFoundry.CloudController.V2.Client.Data;
using Microsoft.Build.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudFoundry.Build.Tasks
{
    public class UnbindRoute:BaseTask
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

            logger = new Microsoft.Build.Utilities.TaskLoggingHelper(this);

            try
            {
                CloudFoundryClient client = InitClient();

                logger.LogMessage("Unbinding route {0} from app {1}", CFRoute, CFAppName);

                string domain = string.Empty;
                string host = string.Empty;
                Utils.ExtractDomainAndHost(CFRoute, out domain, out host);

                if (domain.Length == 0 || host.Length == 0)
                {
                    logger.LogError("Error extracting domain and host information from route {0}", CFRoute);
                    return false;
                }

                PagedResponseCollection<ListAllDomainsDeprecatedResponse> domainInfoList = client.DomainsDeprecated.ListAllDomainsDeprecated().Result;
                ListAllDomainsDeprecatedResponse domainInfo = domainInfoList.Where(o => o.Name == domain).FirstOrDefault();

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

                PagedResponseCollection<ListAllRoutesForAppResponse> routeList = client.Apps.ListAllRoutesForApp(appGuid).Result;

                ListAllRoutesForAppResponse routeInfo = routeList.Where(o => o.Host == host && o.DomainGuid == new Guid(domainInfo.EntityMetadata.Guid)).FirstOrDefault();

                if (routeInfo == null)
                {
                    logger.LogError("Route {0} not found in {1}'s routes", CFRoute, CFAppName);
                    return false;
                }

                client.Routes.RemoveAppFromRoute(new Guid(routeInfo.EntityMetadata.Guid), appGuid).Wait();
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
