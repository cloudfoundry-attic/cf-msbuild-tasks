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

    public class DeleteRoutes : BaseTask
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

                PagedResponseCollection<ListAllDomainsDeprecatedResponse> domainInfoList = client.DomainsDeprecated.ListAllDomainsDeprecated().Result;

                foreach (string domain in app.Domains)
                {
                    foreach (string host in app.Hosts)
                    {
                        ListAllDomainsDeprecatedResponse domainInfo = domainInfoList.Where(o => o.Name == domain).FirstOrDefault();
                        var routeGuid = Utils.GetRouteGuid(client, host, domainInfo.EntityMetadata.Guid.ToGuid());
                        if (routeGuid.HasValue)
                        {
                            client.Routes.DeleteRoute(routeGuid.Value).Wait();
                        }
                        else
                        {
                            Logger.LogError("Route {0}.{1} not found", host, domain);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                this.Logger.LogError("Delete Route failed", exception);
                return false;
            }

            return true;
        }
    }
}
