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
        public string Route { get; set; }

        [Required]
        public string AppName { get; set; }

        [Required]
        public string Space { get; set; }

        public override bool Execute()
        {

            logger = new Microsoft.Build.Utilities.TaskLoggingHelper(this);

            CloudFoundryClient client = InitClient();

            logger.LogMessage("Unbinding route {0} from app {1}", Route, AppName);

            string domain=string.Empty;
            string host = string.Empty;
            Utils.ExtractDomainAndHost(Route, out domain, out host);

            if (domain.Length == 0 || host.Length == 0)
            {
                logger.LogError("Error extracting domain and host information from route {0}", Route);
                return false;
            }

            PagedResponseCollection<ListAllDomainsDeprecatedResponse> domainInfoList = client.DomainsDeprecated.ListAllDomainsDeprecated().Result;
            ListAllDomainsDeprecatedResponse domainInfo = domainInfoList.Where(o => o.Name == domain).FirstOrDefault();

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

            PagedResponseCollection<ListAllAppsForSpaceResponse> appList = client.Spaces.ListAllAppsForSpace(spaceGuid, new RequestOptions() { Query = "name:" + AppName }).Result;
            if (appList.Count() > 1)
            {
                logger.LogError("There are more applications named {0} in space {1}", AppName, Space);
                return false;
            }

            Guid appGuid = new Guid(appList.FirstOrDefault().EntityMetadata.Guid);

            PagedResponseCollection<ListAllRoutesForAppResponse> routeList = client.Apps.ListAllRoutesForApp(appGuid).Result;

            ListAllRoutesForAppResponse routeInfo =  routeList.Where(o => o.Host == host && o.DomainGuid == new Guid(domainInfo.EntityMetadata.Guid)).FirstOrDefault();

            if (routeInfo == null)
            {
                logger.LogError("Route {0} not found in {1}'s routes", Route, AppName);
                return false;
            }

            client.Routes.RemoveAppFromRoute(new Guid(routeInfo.EntityMetadata.Guid), appGuid).Wait();

            return true;
        }

      
    }
}
