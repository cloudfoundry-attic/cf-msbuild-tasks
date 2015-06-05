namespace CloudFoundry.Build.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml;
    using System.Xml.Serialization;
    using CloudFoundry.CloudController.V2.Client;
    using CloudFoundry.CloudController.V2.Client.Data;
    using YamlDotNet.Serialization;

    internal static class Utils
    {
        internal static Guid? GetSpaceGuid(CloudFoundryClient client, TaskLogger logger, string cforganization, string cfspace)
        {
            Guid? spaceGuid = null;
            PagedResponseCollection<ListAllOrganizationsResponse> orgList = client.Organizations.ListAllOrganizations(new RequestOptions() { Query = "name:" + cforganization }).Result;

            if (orgList.Count() > 1)
            {
                logger.LogError("There are more than one organization with name {0}, organization names need to be unique", cforganization);
                return null;
            }

            ListAllOrganizationsResponse orgInfo = orgList.FirstOrDefault();
            if (orgInfo != null)
            {
                PagedResponseCollection<ListAllSpacesForOrganizationResponse> spaceList = client.Organizations.ListAllSpacesForOrganization(orgInfo.EntityMetadata.Guid.ToNullableGuid(), new RequestOptions() { Query = "name:" + cfspace }).Result;

                if (spaceList.Count() > 1)
                {
                    logger.LogError("There are more than one space with name {0} in organization {1}", cfspace, cforganization);
                    return null;
                }

                if (spaceList.FirstOrDefault() != null)
                {
                    spaceGuid = new Guid(spaceList.FirstOrDefault().EntityMetadata.Guid);
                }
                else
                {
                    logger.LogError("Space {0} not found", cfspace);
                    return null;
                }
            }
            else
            {
                logger.LogError("Organization {0} not found", cforganization);
                return null;
            }

            return spaceGuid;
        }

        internal static Guid? GetAppGuid(CloudFoundryClient client, string appName, Guid spaceGuid)
        {
            PagedResponseCollection<ListAllAppsForSpaceResponse> apps = client.Spaces.ListAllAppsForSpace(spaceGuid, new RequestOptions() { Query = "name:" + appName }).Result;

            if (apps.Count() > 0)
            {
                var appGuid = apps.FirstOrDefault().EntityMetadata.Guid;
                return appGuid.ToGuid();
            }

            return null;
        }

        internal static Guid? GetRouteGuid(CloudFoundryClient client, string hostname, Guid domain_guid)
        {
            var routes = client.Routes.ListAllRoutes(new RequestOptions() { Query = string.Format(CultureInfo.InvariantCulture, "host:{0}&domain_guid:{1}", hostname, domain_guid) }).Result;
            if (routes.Count() > 0)
            {
                var routeGuid = routes.FirstOrDefault().EntityMetadata.Guid;
                return routeGuid.ToGuid();
            }
        
            return null;
        }

        internal static Guid? GetServiceGuid(CloudFoundryClient client, string serviceName, Guid spaceGuid)
        {
            var serviceInstancesList = client.Spaces.ListAllServiceInstancesForSpace(spaceGuid, new RequestOptions() { Query = "name:" + serviceName }).Result;

            if (serviceInstancesList.Count() > 0)
            {
                var serviceInstanceGuid = serviceInstancesList.FirstOrDefault().EntityMetadata.Guid;
                return serviceInstanceGuid.ToGuid();
            }
            
            return null;
        }
    }
}
