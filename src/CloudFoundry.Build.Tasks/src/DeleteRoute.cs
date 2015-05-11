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
    public class DeleteRoute : BaseTask
    {
        [Required]
        public string CFRoute { get; set; }

        public override bool Execute()
        {

            logger = new Microsoft.Build.Utilities.TaskLoggingHelper(this);

            try
            {
                CloudFoundryClient client = InitClient();

                logger.LogMessage("Deleting route {0}", CFRoute);

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

                var routeList = client.Routes.ListAllRoutes(new RequestOptions() { Query = string.Format(CultureInfo.InvariantCulture, "host:{0}&domain_guid:{1}", host, domainInfo.EntityMetadata.Guid) }).Result;

                if (routeList.Count() > 1)
                {
                    logger.LogError("There is more than one route that matches for deletion of route {0}", CFRoute);
                    return false;
                }

                client.Routes.DeleteRoute(new Guid(routeList.FirstOrDefault().EntityMetadata.Guid)).Wait();
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
