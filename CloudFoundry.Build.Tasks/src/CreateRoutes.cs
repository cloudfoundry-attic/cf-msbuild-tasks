using CloudFoundry.CloudController.Common.Exceptions;
using CloudFoundry.CloudController.V2;
using CloudFoundry.CloudController.V2.Client.Data;
using CloudFoundry.UAA;
using Microsoft.Build.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudFoundry.Build.Tasks
{
    public class CreateRoutes : BaseTask
    {
        [Required]
        public String[] Routes { get; set; }

        [Required]
        public String Space { get; set; }

        [Output]
        public String[] RouteGuids { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public override bool Execute()
        { 
            logger = new Microsoft.Build.Utilities.TaskLoggingHelper(this);

            CloudFoundryClient client = InitClient();

            Guid? spaceGuid = null;

            if (Space.Length > 0)
            {
                PagedResponseCollection<ListAllSpacesResponse> spaceList = client.Spaces.ListAllSpaces(new RequestOptions() { Query = "name:" + Space }).Result;

                spaceGuid = new Guid(spaceList.FirstOrDefault().EntityMetadata.Guid);
            }

            List<string> createdGuid = new List<string>();
            PagedResponseCollection<ListAllDomainsDeprecatedResponse> domainInfoList = client.DomainsDeprecated.ListAllDomainsDeprecated().Result;

            if (spaceGuid.HasValue)
            {
                foreach (String url in Routes)
                {
                    logger.LogMessage("Creating route {0}", url);
                    string domain = string.Empty;
                    string host = string.Empty;
                    Utils.ExtractDomainAndHost(url, out domain, out host);

                    if (domain.Length == 0 || host.Length == 0)
                    {
                        logger.LogError("Error extracting domain and host information from route {0}", url);
                        continue;
                    }

                    ListAllDomainsDeprecatedResponse domainInfo = domainInfoList.Where(o => o.Name == domain).FirstOrDefault();
                    CreateRouteRequest req = new CreateRouteRequest();
                    req.DomainGuid = new Guid(domainInfo.EntityMetadata.Guid);
                    req.SpaceGuid = spaceGuid;
                    req.Host = host;
                    try
                    {
                        CreateRouteResponse response = client.Routes.CreateRoute(req).Result;
                        createdGuid.Add(response.EntityMetadata.Guid);
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
                RouteGuids = createdGuid.ToArray();
            }
            else
            {
                logger.LogError("Space {0} not found", Space);
                return false;
            }

            return true;
        }
    }
}
