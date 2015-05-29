using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.QualityTools.Testing.Fakes;
using System.Threading.Tasks;
using CloudFoundry.UAA;
using CloudFoundry.Build.Tasks.Test.Properties;
using CloudFoundry.CloudController.V2.Client.Data;
using CloudFoundry.CloudController.V2.Client;

namespace CloudFoundry.Build.Tasks.Test
{
    [TestClass]
    public class BindServicesTaskTest
    {
        [TestMethod]
        public void BindServices_Test()
        {
            using (ShimsContext.Create())
            {
                CloudFoundry.CloudController.V2.Client.Fakes.ShimCloudFoundryClient.AllInstances.LoginCloudCredentials = TestUtils.CustomLogin;

                CloudFoundry.CloudController.V2.Client.Base.Fakes.ShimAbstractServiceBindingsEndpoint.AllInstances.CreateServiceBindingCreateServiceBindingRequest = CustomServiceBinding;

                CloudFoundry.Manifests.Fakes.ShimManifestDiskRepository.ReadManifestString = TestUtils.CustomReadManifest;

                CloudFoundry.Manifests.Fakes.ShimManifest.AllInstances.Applications = TestUtils.CustomManifestApplications;

                CloudFoundry.CloudController.V2.Client.Fakes.ShimPagedResponseCollection<ListAllOrganizationsResponse>.AllInstances.ResourcesGet = TestUtils.CustomListAllOrganizationsResponse;

                CloudFoundry.CloudController.V2.Client.Base.Fakes.ShimAbstractOrganizationsEndpoint.AllInstances.ListAllOrganizationsRequestOptions = TestUtils.CustomListAllOrganizations;

                CloudFoundry.CloudController.V2.Client.Fakes.ShimPagedResponseCollection<ListAllSpacesForOrganizationResponse>.AllInstances.ResourcesGet = TestUtils.CustomListAllSpacesForOrganizationResponse;

                CloudFoundry.CloudController.V2.Client.Base.Fakes.ShimAbstractOrganizationsEndpoint.AllInstances.ListAllSpacesForOrganizationNullableOfGuidRequestOptions = TestUtils.CustomListAllSpacesForOrganization;

                CloudFoundry.CloudController.V2.Client.Fakes.ShimPagedResponseCollection<ListAllAppsForSpaceResponse>.AllInstances.ResourcesGet = TestUtils.CusomListAllAppsForSpacePagedResponse;

                CloudFoundry.CloudController.V2.Client.Base.Fakes.ShimAbstractSpacesEndpoint.AllInstances.ListAllAppsForSpaceNullableOfGuidRequestOptions = TestUtils.CustomListAllAppsForSpace;

                CloudController.V2.Client.Base.Fakes.ShimAbstractDomainsDeprecatedEndpoint.AllInstances.ListAllDomainsDeprecated = TestUtils.CustomListAllDomains;

                CloudFoundry.CloudController.V2.Client.Fakes.ShimPagedResponseCollection<ListAllDomainsDeprecatedResponse>.AllInstances.ResourcesGet = TestUtils.CustomGetDomains;

                CloudFoundry.CloudController.V2.Client.Fakes.ShimPagedResponseCollection<ListAllRoutesResponse>.AllInstances.ResourcesGet = TestUtils.CustomListAllRoutesResponse;

                CloudFoundry.CloudController.V2.Client.Base.Fakes.ShimAbstractRoutesEndpoint.AllInstances.ListAllRoutesRequestOptions = TestUtils.CustomListAllRoutes;

                CloudFoundry.CloudController.V2.Client.Fakes.ShimPagedResponseCollection<ListAllServiceInstancesForSpaceResponse>.AllInstances.ResourcesGet = TestUtils.CustomListAllServiceInstancesForSpaceResponse;

                CloudFoundry.CloudController.V2.Client.Base.Fakes.ShimAbstractSpacesEndpoint.AllInstances.ListAllServiceInstancesForSpaceNullableOfGuidRequestOptions = TestUtils.CustomListAllServiceInstancesForSpace;

                TestUtils.InitTestMetadata();

                BindServices task = new BindServices();
                task.CFSpace = "TestSpace";
                task.CFOrganization = "TestOrg";
                task.CFManifest = Settings.Default.CFManifest;
                task.CFUser = Settings.Default.User;
                task.CFPassword = Settings.Default.Password;
                task.CFServerUri = Settings.Default.ServerUri;
                task.BuildEngine = new FakeBuildEngine();
                task.CFSavedPassword = false;
                task.Execute();

                Assert.AreEqual(task.CFBindingGuids.Length, 1);
            }

        }

        private Task<CreateServiceBindingResponse> CustomServiceBinding(CloudController.V2.Client.Base.AbstractServiceBindingsEndpoint arg1, CreateServiceBindingRequest arg2)
        {
            return Task.Factory.StartNew<CreateServiceBindingResponse>(() =>
            {
                return new CreateServiceBindingResponse() { EntityMetadata = new Metadata() };
            });
        }
    }
}
