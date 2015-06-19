using CloudFoundry.CloudController.V2.Client;
using CloudFoundry.CloudController.V2.Client.Data;
using Microsoft.Build.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CloudFoundry.Build.Tasks
{
    public class Validate : BaseTask
    {
        [Required]
        public string CFAppName { get; set; }

        [Required]
        public string CFOrganization { get; set; }

        [Required]
        public string CFSpace { get; set; }

        [Required]
        public string CFStack { get; set; }

        [Required]
        public String[] CFRoutes { get; set; }

        public string CFServices { get; set; }

        public override bool Execute()
        {
            logger = new Microsoft.Build.Utilities.TaskLoggingHelper(this);
            CloudFoundryClient client = InitClient();

            PagedResponseCollection<ListAllStacksResponse> stackList = client.Stacks.ListAllStacks().Result;

            var stackInfo = stackList.Where(o => o.Name == CFStack).FirstOrDefault();

            if (stackInfo == null)
            {
                logger.LogError("Stack {0} not found", CFStack);
                return false;
            }

            Guid? spaceGuid = Utils.GetSpaceGuid(client, logger, CFOrganization, CFSpace);

            if (spaceGuid.HasValue == false)
            {
                logger.LogError("Invalid space and organization");
                return false;
            }

            PagedResponseCollection<ListAllDomainsDeprecatedResponse> domainInfoList = client.DomainsDeprecated.ListAllDomainsDeprecated().Result;

            foreach (String Route in CFRoutes)
            {
                foreach (var url in Route.Split(';'))
                {
                    logger.LogMessage("Validating route {0}", url);
                    string domain = string.Empty;
                    string host = string.Empty;
                    Utils.ExtractDomainAndHost(url, out domain, out host);

                    if (domain.Length == 0 || host.Length == 0)
                    {
                        logger.LogError("Error extracting domain and host information from route {0}", url);
                        continue;
                    }

                    ListAllDomainsDeprecatedResponse domainInfo = domainInfoList.Where(o => o.Name.ToUpperInvariant() == domain.ToUpperInvariant()).FirstOrDefault();

                    if (domainInfo == null)
                    {
                        logger.LogError("Domain {0} not found", domain);
                        return false;
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(CFServices) == false)
            {
                if (ValidateServices(client, CFServices) == false)
                {
                    logger.LogError("Error validating services");
                    return false;
                }
            }


            return true;
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private bool ValidateServices(CloudFoundryClient client, string CFServices)
        {
            List<ProvisionedService> servicesList = new List<ProvisionedService>();
            try
            {
                string[] provServs = CFServices.Split(';');

                foreach (string service in provServs)
                {
                    if (string.IsNullOrWhiteSpace(service) == false)
                    {
                        string[] serviceInfo = service.Split(',');

                        if (serviceInfo.Length != 3)
                        {
                            logger.LogError("Invalid service information in {0}", service);
                            continue;
                        }

                        ProvisionedService serviceDetails = new ProvisionedService();

                        serviceDetails.Name = serviceInfo[0].Trim();
                        serviceDetails.Type = serviceInfo[1].Trim();
                        serviceDetails.Plan = serviceInfo[2].Trim();

                        servicesList.Add(serviceDetails);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogErrorFromException(ex);
                logger.LogWarning("Error trying to obtain service information, trying to deserialize as xml");
                servicesList = Utils.Deserialize<List<ProvisionedService>>(CFServices);
            }

            foreach (ProvisionedService service in servicesList)
            {
                logger.LogMessage("Validating {0} service {1}", service.Type, service.Name);

                PagedResponseCollection<ListAllServicesResponse> allServicesList = client.Services.ListAllServices(new RequestOptions() { Query = "label:" + service.Type }).Result;

                if (allServicesList.Count() < 1)
                {
                    logger.LogError("Invalid service type {0}", service.Type);
                    return false;
                }

                foreach (var serviceInfo in allServicesList)
                {
                    var planList = client.Services.ListAllServicePlansForService(new Guid(serviceInfo.EntityMetadata.Guid)).Result;

                    var plan = planList.Where(o => o.Name == service.Plan).FirstOrDefault();

                    if (plan != null)
                    {
                        break;
                    }
                    else
                    {
                        logger.LogError("Invalid plan {2} for service {0} - {1}", service.Name, service.Type, service.Plan);
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
