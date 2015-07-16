namespace CloudFoundry.Build.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using CloudFoundry.CloudController.Common.Exceptions;
    using CloudFoundry.CloudController.V2.Client;
    using CloudFoundry.CloudController.V2.Client.Data;
    using CloudFoundry.UAA;
    using Microsoft.Build.Framework;

    public class CreateRoutes : BaseTask
    {  
        [Required]
        public string CFOrganization { get; set; }

        [Required]
        public string CFSpace { get; set; }

        [Output]
        public string[] CFRouteGuids { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Coupling needed")]
        public override bool Execute()
        {
            this.CFOrganization = this.CFOrganization.Trim();
            this.CFSpace = this.CFSpace.Trim();

            var app = LoadAppFromManifest();
            this.Logger = new TaskLogger(this);

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

                List<string> createdGuid = new List<string>();
                PagedResponseCollection<ListAllDomainsDeprecatedResponse> domainInfoList = client.DomainsDeprecated.ListAllDomainsDeprecated().Result;

                if (spaceGuid.HasValue)
                {
                    foreach (string domain in app.Domains)
                    {
                            foreach (string host in app.Hosts)
                            {
                                if (string.IsNullOrWhiteSpace(host) == false)
                                {
                                    Logger.LogMessage("Creating route {0}.{1}", host, domain);

                                    ListAllDomainsDeprecatedResponse domainInfo = domainInfoList.Where(o => o.Name == domain).FirstOrDefault();

                                    if (domainInfo == null)
                                    {
                                        Logger.LogError("Domain {0} not found", domain);
                                        continue;
                                    }

                                    this.CreateRoute(client, spaceGuid, createdGuid, host, domainInfo);
                                }
                            }
                    }

                    this.CFRouteGuids = createdGuid.ToArray();
                }
                else
                {
                    Logger.LogError("Space {0} not found", this.CFSpace);
                    return false;
                }
            }
            catch (Exception exception)
            {
                this.Logger.LogError("Create Route failed", exception);
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
                    Logger.LogMessage("Route {0}.{1} already exists", routeInfo.Host, domainInfo.Name);
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
                        Logger.LogWarning(e.Message);
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
