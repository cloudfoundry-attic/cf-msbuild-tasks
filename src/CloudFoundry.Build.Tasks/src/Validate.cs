namespace CloudFoundry.Build.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using CloudFoundry.CloudController.V2.Client;
    using CloudFoundry.CloudController.V2.Client.Data;
    using Microsoft.Build.Framework;

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
        public string[] CFRoutes { get; set; }

        public string CFServices { get; set; }

        public override bool Execute()
        {
            this.Logger = new TaskLogger(this);
            try
            {
                CloudFoundryClient client = InitClient();

                PagedResponseCollection<ListAllStacksResponse> stackList = client.Stacks.ListAllStacks().Result;

                var stackInfo = stackList.Where(o => o.Name == this.CFStack).FirstOrDefault();

                if (stackInfo == null)
                {
                    this.Logger.LogError("Stack {0} not found", this.CFStack);
                    return false;
                }

                Guid? spaceGuid = Utils.GetSpaceGuid(client, this.Logger, this.CFOrganization, this.CFSpace);

                if (spaceGuid.HasValue == false)
                {
                    this.Logger.LogError("Invalid space and organization");
                    return false;
                }

                PagedResponseCollection<ListAllDomainsDeprecatedResponse> domainInfoList = client.DomainsDeprecated.ListAllDomainsDeprecated().Result;

                foreach (string route in this.CFRoutes)
                {
                    foreach (var url in route.Split(';'))
                    {
                        this.Logger.LogMessage("Validating route {0}", url);
                        string domain = string.Empty;
                        string host = string.Empty;
                        Utils.ExtractDomainAndHost(url, out domain, out host);

                        if (string.IsNullOrWhiteSpace(domain) || string.IsNullOrWhiteSpace(host))
                        {
                            this.Logger.LogError("Error extracting domain and host information from route {0}", url);
                            continue;
                        }

                        ListAllDomainsDeprecatedResponse domainInfo = domainInfoList.Where(o => o.Name.ToUpperInvariant() == domain.ToUpperInvariant()).FirstOrDefault();

                        if (domainInfo == null)
                        {
                            this.Logger.LogError("Domain {0} not found", domain);
                            return false;
                        }
                    }
                }

                if (string.IsNullOrWhiteSpace(this.CFServices) == false)
                {
                    if (this.ValidateServices(client, this.CFServices) == false)
                    {
                        this.Logger.LogError("Error validating services");
                        return false;
                    }
                }
            }
            catch (Exception exception)
            {
                this.Logger.LogError("Validate failed", exception);
                return false;
            }

            return true;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "General exceptions logged to file")]
        private bool ValidateServices(CloudFoundryClient client, string cfservices)
        {
            List<ProvisionedService> servicesList = new List<ProvisionedService>();
            try
            {
                string[] provServs = cfservices.Split(';');

                foreach (string service in provServs)
                {
                    if (string.IsNullOrWhiteSpace(service) == false)
                    {
                        string[] serviceInfo = service.Split(',');

                        if (serviceInfo.Length != 3)
                        {
                            this.Logger.LogError("Invalid service information in {0}", service);
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
                this.Logger.LogErrorFromException(ex);
                this.Logger.LogWarning("Error trying to obtain service information, trying to deserialize as xml");
                servicesList = Utils.Deserialize<List<ProvisionedService>>(cfservices);
            }

            foreach (ProvisionedService service in servicesList)
            {
                this.Logger.LogMessage("Validating {0} service {1}", service.Type, service.Name);

                PagedResponseCollection<ListAllServicesResponse> allServicesList = client.Services.ListAllServices(new RequestOptions() { Query = "label:" + service.Type }).Result;

                if (allServicesList.Count() < 1)
                {
                    this.Logger.LogError("Invalid service type {0}", service.Type);
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
                        this.Logger.LogError("Invalid plan {2} for service {0} - {1}", service.Name, service.Type, service.Plan);
                        return false;
                    }
                }
            }

            return true;
        }
    }
}