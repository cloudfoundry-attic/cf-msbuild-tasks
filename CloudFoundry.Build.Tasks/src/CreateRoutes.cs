using CloudFoundry.CloudController.Common.Exceptions;
using CloudFoundry.CloudController.V2;
using CloudFoundry.CloudController.V2.Client.Data;
using CloudFoundry.UAA;
using Microsoft.Build.Framework;
using System;
using System.Collections.Generic;
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

        public override bool Execute()
        {
          
            logger = new Microsoft.Build.Utilities.TaskLoggingHelper(this);

            CloudFoundryClient client = InitClient();

            Guid? spaceGuid = null;

            if (Space != string.Empty)
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
                    string domain = url.Substring(url.IndexOf('.') + 1);

                    ListAllDomainsDeprecatedResponse domainInfo = domainInfoList.Where(o => o.Name == domain).FirstOrDefault();
                    CreateRouteRequest req = new CreateRouteRequest();
                    req.DomainGuid = new Guid(domainInfo.EntityMetadata.Guid);
                    req.SpaceGuid = spaceGuid;
                    req.Host = url.Split('.').First().ToLower();
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
                                throw ex;
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
