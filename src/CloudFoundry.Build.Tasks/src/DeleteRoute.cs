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

    public class DeleteRoute : BaseTask
    {
        [Required]
        public string CFRoute { get; set; }

        public override bool Execute()
        {
            this.Logger = new TaskLogger(this);
            try
            {
                CloudFoundryClient client = InitClient();

                Logger.LogMessage("Deleting route {0}", this.CFRoute);

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

                var routeList = client.Routes.ListAllRoutes(new RequestOptions() { Query = string.Format(CultureInfo.InvariantCulture, "host:{0}&domain_guid:{1}", host, domainInfo.EntityMetadata.Guid) }).Result;

                if (routeList.Count() > 1)
                {
                    Logger.LogError("There is more than one route that matches for deletion of route {0}", this.CFRoute);
                    return false;
                }

                client.Routes.DeleteRoute(new Guid(routeList.FirstOrDefault().EntityMetadata.Guid)).Wait();
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
