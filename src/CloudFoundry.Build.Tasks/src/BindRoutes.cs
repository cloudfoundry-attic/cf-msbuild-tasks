namespace CloudFoundry.Build.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using CloudFoundry.CloudController.V2.Client;
    using CloudFoundry.CloudController.V2.Client.Data;
    using CloudFoundry.UAA;
    using Microsoft.Build.Framework;

    public class BindRoutes : BaseTask
    {
        [Required]
        public string CFOrganization { get; set; }

        [Required]
        public string CFSpace { get; set; }

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

                var appGuid = Utils.GetAppGuid(client, app.Name, spaceGuid.Value);

                if (appGuid.HasValue)
                {
                    PagedResponseCollection<ListAllDomainsDeprecatedResponse> domainInfoList = client.DomainsDeprecated.ListAllDomainsDeprecated().Result;

                    foreach (string domain in app.Domains)
                    {
                        foreach (string host in app.Hosts)
                        { 
                            ListAllDomainsDeprecatedResponse domainInfo = domainInfoList.Where(o => o.Name == domain).FirstOrDefault();

                            if (domainInfo == null)
                            {
                                Logger.LogError("Domain {0} not found", domain);
                                continue;
                            }

                            var routeGuid = Utils.GetRouteGuid(client, host, domainInfo.EntityMetadata.Guid.ToGuid());

                            if (routeGuid.HasValue)
                            {
                                Logger.LogMessage("Binding route {0}.{1} to application {2}", host, domain, app.Name);

                                client.Apps.AssociateRouteWithApp(appGuid.Value, routeGuid.Value).Wait();
                            }
                            else
                            {
                                Logger.LogError("Could not find route {0}.{1}", host, domain);
                            }
                        }
                    }
                }
                else
                {
                    Logger.LogError("App {0} not found on space {1}", app.Name, this.CFSpace);
                }
            }
            catch (Exception exception)
            {
                this.Logger.LogError("Bind Routes failed", exception);
                return false;
            }

            return true;
        }
    }
}
