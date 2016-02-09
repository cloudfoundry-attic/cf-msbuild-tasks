using CloudFoundry.CloudController.V2;
using CloudFoundry.CloudController.V2.Client;
using CloudFoundry.CloudController.V2.Client.Data;
using CloudFoundry.UAA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudFoundry.Build.Tasks.Test
{
    internal static class TestUtils
    {

        private static Guid? testGuid = Guid.NewGuid();

        internal static void InitTestMetadata()
        {
            CloudFoundry.CloudController.V2.Client.Fakes.ShimMetadata.AllInstances.GuidGet = CustomMetada;
        }

        private static EntityGuid CustomMetada(Metadata arg1)
        {
            return EntityGuid.FromGuid(testGuid);
        }

   
        internal static Task<CreateServiceInstanceResponse> CustomCreateServiceInstance(CloudController.V2.Client.Base.AbstractServiceInstancesEndpoint arg1, CreateServiceInstanceRequest arg2)
        {
            return Task.Factory.StartNew<CreateServiceInstanceResponse>(() =>
            {
                return new CreateServiceInstanceResponse() { EntityMetadata = new Metadata() };
            });
        }

        internal static List<ListAllServicePlansForServiceResponse> CustomListServicePlansResponse(PagedResponseCollection<ListAllServicePlansForServiceResponse> arg1)
        {
            return new List<ListAllServicePlansForServiceResponse>() { new ListAllServicePlansForServiceResponse() { Name = "myPlan", EntityMetadata = new Metadata() } };
        }

        internal static Task<PagedResponseCollection<ListAllServicePlansForServiceResponse>> CustomListServicePlans(CloudController.V2.Client.Base.AbstractServicesEndpoint arg1, Guid? arg2)
        {
            return Task.Factory.StartNew<PagedResponseCollection<ListAllServicePlansForServiceResponse>>(new Func<PagedResponseCollection<ListAllServicePlansForServiceResponse>>(() =>
            {
                return new PagedResponseCollection<ListAllServicePlansForServiceResponse>();
            }));
        }

        internal static List<ListAllServicesResponse> CustomListAllServicesResponse(PagedResponseCollection<ListAllServicesResponse> arg1)
        {
            return new List<ListAllServicesResponse>() { new ListAllServicesResponse() { Label = "myType", EntityMetadata = new Metadata() } };
        }

        internal static Task<PagedResponseCollection<ListAllServicesResponse>> CustomListAllServices(CloudController.V2.Client.Base.AbstractServicesEndpoint arg1, RequestOptions arg2)
        {
            return Task.Factory.StartNew<PagedResponseCollection<ListAllServicesResponse>>(new Func<PagedResponseCollection<ListAllServicesResponse>>(() =>
            {
                return new PagedResponseCollection<ListAllServicesResponse>();
            }));
        }

        internal static List<ListAllSpacesResponse> CustomGetPaged(PagedResponseCollection<ListAllSpacesResponse> arg1)
        {
            List<ListAllSpacesResponse> response1 = new List<ListAllSpacesResponse>() { new ListAllSpacesResponse() { EntityMetadata = new Metadata() } };
            return response1;
        }


        internal static Task<PagedResponseCollection<ListAllSpacesResponse>> CustomListAllSpaces(CloudController.V2.Client.Base.AbstractSpacesEndpoint arg1, RequestOptions arg2)
        {
            return Task.Factory.StartNew<PagedResponseCollection<ListAllSpacesResponse>>(new Func<PagedResponseCollection<ListAllSpacesResponse>>(
                () =>
                {
                    PagedResponseCollection<ListAllSpacesResponse> collection = new PagedResponseCollection<ListAllSpacesResponse>();

                    return collection;
                }
              ));
        }

        // { Token = new UAA.Authentication.Token() { RefreshToken = "mytoken" } } - Token is read-only and cannot be assigned to from shim context without fakeing uaa
        internal static Task<AuthenticationContext> CustomLogin(CloudController.V2.Client.CloudFoundryClient arg1, CloudCredentials arg2)
        {
            return Task.Factory.StartNew<AuthenticationContext>(() => { return new AuthenticationContext(); });
        }


        internal static Task<CreateRouteResponse> CustomCreateRoute(CloudController.V2.Client.Base.AbstractRoutesEndpoint arg1, CreateRouteRequest arg2)
        {
            return Task.Factory.StartNew<CreateRouteResponse>(new Func<CreateRouteResponse>(() => { return new CreateRouteResponse() { EntityMetadata = new Metadata() }; }));
        }

        internal static List<ListAllDomainsDeprecatedResponse> CustomGetDomains(PagedResponseCollection<ListAllDomainsDeprecatedResponse> arg1)
        {
            return new List<ListAllDomainsDeprecatedResponse>() { new ListAllDomainsDeprecatedResponse() { Name = "domain.com", EntityMetadata = new Metadata() } };
        }

        internal static Task<PagedResponseCollection<ListAllDomainsDeprecatedResponse>> CustomListAllDomains(CloudController.V2.Client.Base.AbstractDomainsDeprecatedEndpoint arg1)
        {
            return Task.Factory.StartNew<PagedResponseCollection<ListAllDomainsDeprecatedResponse>>(new Func<PagedResponseCollection<ListAllDomainsDeprecatedResponse>>(() =>
            {
                return new PagedResponseCollection<ListAllDomainsDeprecatedResponse>();
            }));
        }


        internal static Task<CreateAppResponse> CustomCreateApp(CloudController.V2.Client.Base.AbstractAppsEndpoint arg1, CreateAppRequest arg2)
        {
           return Task.Factory.StartNew<CreateAppResponse>(() =>
            {
                return new CreateAppResponse() { EntityMetadata = new Metadata(), Name = "testApp" };

            });
        }

        internal static List<ListAllStacksResponse> CustomGetStacks(PagedResponseCollection<ListAllStacksResponse> arg1)
        {
            List<ListAllStacksResponse> response1 = new List<ListAllStacksResponse>()
            {
                new ListAllStacksResponse(){ Name="testStack", EntityMetadata = new Metadata()}
            };
            return response1;
        }

        internal static Task<PagedResponseCollection<ListAllStacksResponse>> CustomListAllStacks(CloudController.V2.Client.Base.AbstractStacksEndpoint arg1)
        {
            return Task.Factory.StartNew<PagedResponseCollection<ListAllStacksResponse>>(new Func<PagedResponseCollection<ListAllStacksResponse>>(
                () =>
                {
                    PagedResponseCollection<ListAllStacksResponse> collection = new PagedResponseCollection<ListAllStacksResponse>();
                    return collection;
                }
                ));
        }

        internal static Task<UpdateAppResponse> CustomUpdateApp(CloudController.V2.Client.Base.AbstractAppsEndpoint arg1, Guid? arg2, UpdateAppRequest arg3)
        {
            return Task.Factory.StartNew<UpdateAppResponse>(() =>
            {
                return new UpdateAppResponse() { Name = "TestApp" };
            });
        }

        internal static Task CustomDeleteApp(CloudController.V2.Client.Base.AbstractAppsEndpoint arg1, Guid? arg2)
        {
            var task = Task.Run(() => { });
            return task;
        }

        internal static Task<PagedResponseCollection<ListAllAppsForSpaceResponse>> CustomListAllAppsForSpace(CloudController.V2.Client.Base.AbstractSpacesEndpoint arg1, Guid? arg2, RequestOptions arg3)
        {
            return Task.Factory.StartNew<PagedResponseCollection<ListAllAppsForSpaceResponse>>(() =>
            {
                return new PagedResponseCollection<ListAllAppsForSpaceResponse>();
            });
        }


        internal static List<ListAllAppsForSpaceResponse> CusomListAllAppsForSpacePagedResponseCreate(PagedResponseCollection<ListAllAppsForSpaceResponse> arg1)
        {
            return new List<ListAllAppsForSpaceResponse>();
        }

        internal static List<ListAllAppsForSpaceResponse> CusomListAllAppsForSpacePagedResponse(PagedResponseCollection<ListAllAppsForSpaceResponse> arg1)
        {
            return new List<ListAllAppsForSpaceResponse>() { new ListAllAppsForSpaceResponse() { Name = "testApp", EntityMetadata = new Metadata() } };
        }

        internal static Task<PagedResponseCollection<ListAllRoutesForAppResponse>> CustomListAllRoutesForApp(CloudController.V2.Client.Base.AbstractAppsEndpoint arg1, Guid? arg2)
        {
            return Task.Factory.StartNew<PagedResponseCollection<ListAllRoutesForAppResponse>>(() =>
            {
                return new PagedResponseCollection<ListAllRoutesForAppResponse>();
            });
        }

        internal static List<ListAllRoutesForAppResponse> CustomListAllRoutesForAppResponse(PagedResponseCollection<ListAllRoutesForAppResponse> arg1)
        {
            return new List<ListAllRoutesForAppResponse>() { new ListAllRoutesForAppResponse() { Host = "test", DomainGuid = testGuid, EntityMetadata = new Metadata() } };
        }

        internal static Task CustomDeleteRoute(CloudController.V2.Client.Base.AbstractRoutesEndpoint arg1, Guid? arg2)
        {
            return Task.Factory.StartNew(() => { });
        }

        internal static Task<PagedResponseCollection<ListAllServiceBindingsForAppResponse>> CustomListAllServiceBindingsForApp(CloudController.V2.Client.Base.AbstractAppsEndpoint arg1, Guid? arg2)
        {
            return Task.Factory.StartNew<PagedResponseCollection<ListAllServiceBindingsForAppResponse>>(() =>
            {
                return new PagedResponseCollection<ListAllServiceBindingsForAppResponse>();
            });
        }

        internal static List<ListAllServiceBindingsForAppResponse> CustomListAllServiceBindingsForAppResponse(PagedResponseCollection<ListAllServiceBindingsForAppResponse> arg1)
        {
            return new List<ListAllServiceBindingsForAppResponse>() { new ListAllServiceBindingsForAppResponse() { ServiceInstanceGuid = Guid.NewGuid(), EntityMetadata=new Metadata() } };
        }

        internal static Task CustomDeleteServiceBinding(CloudController.V2.Client.Base.AbstractServiceBindingsEndpoint arg1, Guid? arg2)
        {
            return Task.Factory.StartNew(() => { });
        }

        internal static Task<PagedResponseCollection<ListAllRoutesResponse>> CustomListAllRoutes(CloudController.V2.Client.Base.AbstractRoutesEndpoint arg1, RequestOptions arg2)
        {
            return Task.Factory.StartNew<PagedResponseCollection<ListAllRoutesResponse>>(() =>
            {
                return new PagedResponseCollection<ListAllRoutesResponse>();
            });
        }

        internal static List<ListAllRoutesResponse> CustomListAllRoutesResponse(PagedResponseCollection<ListAllRoutesResponse> arg1)
        {
            return new List<ListAllRoutesResponse>() { new ListAllRoutesResponse() { EntityMetadata = new Metadata() } };
        }

        internal static Task<PagedResponseCollection<ListAllServiceInstancesForSpaceResponse>> CustomListAllServiceInstances(CloudController.V2.Client.Base.AbstractSpacesEndpoint arg1, Guid? arg2, RequestOptions arg3)
        {
            return Task.Factory.StartNew<PagedResponseCollection<ListAllServiceInstancesForSpaceResponse>>(() =>
            {
                return new PagedResponseCollection<ListAllServiceInstancesForSpaceResponse>();
            });
        }

        internal static List<ListAllServiceInstancesForSpaceResponse> CustomListAllServiceInstancesResponse(PagedResponseCollection<ListAllServiceInstancesForSpaceResponse> arg1)
        {
            return new List<ListAllServiceInstancesForSpaceResponse>() { new ListAllServiceInstancesForSpaceResponse() { EntityMetadata = new Metadata() } };
        }

        internal static Task<RemoveAppFromRouteResponse> CustomRemoveAppFromRoute(CloudController.V2.Client.Base.AbstractRoutesEndpoint arg1, Guid? arg2, Guid? arg3)
        {
            return Task.Factory.StartNew(() => { return new RemoveAppFromRouteResponse(); });
        }

        private static int callNumber = 0;
        internal static Task<GetAppSummaryResponse> CustomGetAppSummary(CloudController.V2.Client.Base.AbstractAppsEndpoint arg1, Guid? arg2)
        {
            return Task.Factory.StartNew<GetAppSummaryResponse>(() => {

                switch (callNumber)
                {
                    case 0:
                        {
                            callNumber += 1;
                            return new GetAppSummaryResponse() { State = "PENDING", RunningInstances = 0 };
                        }
                    case 1:
                        {
                            callNumber += 1;
                            return new GetAppSummaryResponse() { State = "STAGED", RunningInstances = 0 };
                        }
                    case 2:
                        {
                            return new GetAppSummaryResponse() { State = "STARTED", RunningInstances = 1 };
                        }
                    default: { return new GetAppSummaryResponse(); }
                }
            });
        }

        internal static Task<GetV1InfoResponse> CustomGetV1Info(InfoEndpoint arg1)
        {
            return Task.Factory.StartNew<GetV1InfoResponse>(() => { return new GetV1InfoResponse() { AppLogEndpoint = "ws://logs.1.2.3.4.xip.io" }; });
        }

        internal static void CustomStartLogStreamString(Logyard.Client.LogyardLog arg1, string arg2)
        {
         
        }

        internal static Task<PagedResponseCollection<ListAllServiceInstancesResponse>> CustomListAllServiceInstancesPlain(CloudController.V2.Client.Base.AbstractServiceInstancesEndpoint arg1)
        {
            return Task.Factory.StartNew<PagedResponseCollection<ListAllServiceInstancesResponse>>(() => { return new PagedResponseCollection<ListAllServiceInstancesResponse>(); });
        }

        internal static List<ListAllServiceInstancesResponse> CustomListAllServiceInstancesPlainResponse(PagedResponseCollection<ListAllServiceInstancesResponse> arg1)
        {
            return new List<ListAllServiceInstancesResponse>();
        }

        internal static Task<PagedResponseCollection<ListAllOrganizationsResponse>> CustomListAllOrganizations(CloudController.V2.Client.Base.AbstractOrganizationsEndpoint arg1, RequestOptions arg2)
        {
            return Task.Factory.StartNew(() => { return new PagedResponseCollection<ListAllOrganizationsResponse>(); });
        }

        internal static List<ListAllOrganizationsResponse> CustomListAllOrganizationsResponse(PagedResponseCollection<ListAllOrganizationsResponse> arg1)
        {
            return new List<ListAllOrganizationsResponse>() { new ListAllOrganizationsResponse(){ EntityMetadata=new Metadata() }};
        }

        internal static Task<PagedResponseCollection<ListAllSpacesForOrganizationResponse>> CustomListAllSpacesForOrganization(CloudController.V2.Client.Base.AbstractOrganizationsEndpoint arg1, Guid? arg2, RequestOptions arg3)
        {
            return Task.Factory.StartNew(() => { return new PagedResponseCollection<ListAllSpacesForOrganizationResponse>(); });
        }

        internal static List<ListAllSpacesForOrganizationResponse> CustomListAllSpacesForOrganizationResponse(PagedResponseCollection<ListAllSpacesForOrganizationResponse> arg1)
        {
            return new List<ListAllSpacesForOrganizationResponse>() {new ListAllSpacesForOrganizationResponse(){ EntityMetadata=new Metadata(), Name="TestSpace"}};
        }

        internal static EntityGuid CustomMetadataGuidGet(Metadata arg1)
        {
            return EntityGuid.FromGuid(Guid.NewGuid());
        }

        internal static Manifests.Models.Application[] CustomManifestApplications(Manifests.Manifest arg1)
        {
            var app = new Manifests.Models.Application() { Name = "testApp", StackName = "testStack", InstanceCount = 1, Memory = 512, Path = "C:\\" };

            app.Domains.Add("domain.com" );
            app.Hosts.Add("testApp" );
            app.Services.Add("testservice");

            return new Manifests.Models.Application[1]{
               app
            };
        }

        internal static Task<PagedResponseCollection<ListAllServiceInstancesForSpaceResponse>> CustomListAllServiceInstancesForSpace(CloudController.V2.Client.Base.AbstractSpacesEndpoint arg1, Guid? arg2, RequestOptions arg3)
        {
            return Task<PagedResponseCollection<ListAllServiceInstancesForSpaceResponse>>.Factory.StartNew(() => {
                return new PagedResponseCollection<ListAllServiceInstancesForSpaceResponse>();
            });
        }

        internal static List<ListAllServiceInstancesForSpaceResponse> CustomListAllServiceInstancesForSpaceResponse(PagedResponseCollection<ListAllServiceInstancesForSpaceResponse> arg1)
        {
            return new List<ListAllServiceInstancesForSpaceResponse>() { new ListAllServiceInstancesForSpaceResponse() { EntityMetadata = new Metadata() } };
        }

        internal static Manifests.Manifest CustomReadManifest(string arg1)
        {
            return new Manifests.Manifest();
        }

        internal static Task<DeleteServiceInstanceResponse> CustomDeleteServiceInstance(ServiceInstancesEndpoint arg1, Guid? arg2)
        {
            return Task<DeleteServiceInstanceResponse>.Factory.StartNew(() =>
            {
                return new DeleteServiceInstanceResponse();
            });
        }
    }
}
