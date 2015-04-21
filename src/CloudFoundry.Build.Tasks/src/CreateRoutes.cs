using CloudFoundry.CloudController.Common.Exceptions;
using CloudFoundry.CloudController.V2.Client;
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
        public String[] CFRoutes { get; set; }

        [Required]
        public string CFOrganization { get; set; }

        [Required]
        public String CFSpace { get; set; }

        [Output]
        public String[] CFRouteGuids { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public override bool Execute()
        { 
            logger = new Microsoft.Build.Utilities.TaskLoggingHelper(this);

            CloudFoundryClient client = InitClient();

            Guid? spaceGuid = null;

            if (CFSpace.Length > 0 && CFOrganization.Length > 0)
            {

                spaceGuid = Utils.GetSpaceGuid(client, logger, CFOrganization, CFSpace);

                if (spaceGuid == null)
                {
                    return false;
                }
            }

            List<string> createdGuid = new List<string>();
            PagedResponseCollection<ListAllDomainsDeprecatedResponse> domainInfoList = client.DomainsDeprecated.ListAllDomainsDeprecated().Result;

            if (spaceGuid.HasValue)
            {
                foreach (String Route in CFRoutes)
                {
                    if (Route.Contains(';'))
                    {
                        foreach (var url in Route.Split(';'))
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

                            if (domainInfo == null)
                            {
                                logger.LogError("Domain {0} not found", domain);
                                continue;
                            }

                            CreateRoute(client, spaceGuid, createdGuid, host, domainInfo);
                        }
                    }
                    else
                    {
                        string domain = string.Empty;
                        string host = string.Empty;
                        Utils.ExtractDomainAndHost(Route, out domain, out host);

                        if (domain.Length == 0 || host.Length == 0)
                        {
                            logger.LogError("Error extracting domain and host information from route {0}", Route);
                            continue;
                        }
                        ListAllDomainsDeprecatedResponse domainInfo = domainInfoList.Where(o => o.Name == domain).FirstOrDefault();

                        if (domainInfo == null)
                        {
                            logger.LogError("Domain {0} not found", domain);
                            continue;
                        }
                        CreateRoute(client, spaceGuid, createdGuid, host, domainInfo);
                    }
                }
                CFRouteGuids = createdGuid.ToArray();
            }
            else
            {
                logger.LogError("Space {0} not found", CFSpace);
                return false;
            }

            return true;
        }

        private void CreateRoute(CloudFoundryClient client, Guid? spaceGuid, List<string> createdGuid, string host, ListAllDomainsDeprecatedResponse domainInfo)
        {
            CreateRouteRequest req = new CreateRouteRequest();
            req.DomainGuid = new Guid(domainInfo.EntityMetadata.Guid);
            req.SpaceGuid = spaceGuid;
            req.Host = host;
            try
            {

                var routes = client.Routes.ListAllRoutes(new RequestOptions() { Query = string.Format(CultureInfo.InvariantCulture, "host:{0}&domain_guid:{1}", host, domainInfo.EntityMetadata.Guid) }).Result;

                if (routes.Count() > 0)
                {
                    ListAllRoutesResponse routeInfo = routes.FirstOrDefault();
                    logger.LogMessage("Route {0}.{1} already exists", routeInfo.Host, routeInfo.DomainUrl);
                    if (routeInfo != null)
                    {
                        createdGuid.Add(routeInfo.EntityMetadata.Guid);
                    }
                }
                else
                {
                    CreateRouteResponse response = client.Routes.CreateRoute(req).Result;
                    createdGuid.Add(response.EntityMetadata.Guid);
                }
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
}
