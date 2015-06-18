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

                PagedResponseCollection<ListAllStacksResponse> stackList = client.Stacks.ListAllStacks().Result;

                var stackInfo = stackList.Where(o => o.Name == app.StackName).FirstOrDefault();

                if (stackInfo == null)
                {
                    this.Logger.LogError("Stack {0} not found", app.StackName);
                    return false;
                }

                Guid? spaceGuid = Utils.GetSpaceGuid(client, this.Logger, this.CFOrganization, this.CFSpace);

                if (spaceGuid.HasValue == false)
                {
                    this.Logger.LogError("Invalid space and organization");
                    return false;
                }

                PagedResponseCollection<ListAllDomainsDeprecatedResponse> domainInfoList = client.DomainsDeprecated.ListAllDomainsDeprecated().Result;

                foreach (string domain in app.Domains)
                {
                    this.Logger.LogMessage("Validating domain {0}", domain);

                    ListAllDomainsDeprecatedResponse domainInfo = domainInfoList.Where(o => o.Name.ToUpperInvariant() == domain.ToUpperInvariant()).FirstOrDefault();

                    if (domainInfo == null)
                    {
                        this.Logger.LogError("Domain {0} not found", domain);
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
    }
}