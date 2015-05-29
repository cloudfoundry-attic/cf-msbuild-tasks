using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.QualityTools.Testing.Fakes;
using CloudFoundry.Build.Tasks.Test.Properties;
using CloudFoundry.CloudController.V2.Client.Data;

namespace CloudFoundry.Build.Tasks.Test
{
    [TestClass]
    public class UnbindRoutesTaskTest
    {
        [TestMethod]
        public void UnbindRoutes_Test()
        {
            using (ShimsContext.Create())
            {
                CloudFoundry.CloudController.V2.Client.Fakes.ShimCloudFoundryClient.AllInstances.LoginCloudCredentials = TestUtils.CustomLogin;

                CloudFoundry.Manifests.Fakes.ShimManifestDiskRepository.ReadManifestString = TestUtils.CustomReadManifest;

                CloudFoundry.Manifests.Fakes.ShimManifest.AllInstances.Applications = TestUtils.CustomManifestApplications;

                CloudFoundry.CloudController.V2.Client.Base.Fakes.ShimAbstractSpacesEndpoint.AllInstances.ListAllSpacesRequestOptions = TestUtils.CustomListAllSpaces;

                CloudFoundry.CloudController.V2.Client.Fakes.ShimPagedResponseCollection<ListAllSpacesResponse>.AllInstances.ResourcesGet = TestUtils.CustomGetPaged;

                CloudFoundry.CloudController.V2.Client.Fakes.ShimPagedResponseCollection<ListAllAppsForSpaceResponse>.AllInstances.ResourcesGet = TestUtils.CusomListAllAppsForSpacePagedResponse;

                CloudController.V2.Client.Base.Fakes.ShimAbstractDomainsDeprecatedEndpoint.AllInstances.ListAllDomainsDeprecated = TestUtils.CustomListAllDomains;

                CloudFoundry.CloudController.V2.Client.Fakes.ShimPagedResponseCollection<ListAllDomainsDeprecatedResponse>.AllInstances.ResourcesGet = TestUtils.CustomGetDomains;

                CloudFoundry.CloudController.V2.Client.Base.Fakes.ShimAbstractSpacesEndpoint.AllInstances.ListAllAppsForSpaceNullableOfGuidRequestOptions = TestUtils.CustomListAllAppsForSpace;

                CloudFoundry.CloudController.V2.Client.Fakes.ShimPagedResponseCollection<ListAllRoutesForAppResponse>.AllInstances.ResourcesGet = TestUtils.CustomListAllRoutesForAppResponse;

                CloudFoundry.CloudController.V2.Client.Base.Fakes.ShimAbstractAppsEndpoint.AllInstances.ListAllRoutesForAppNullableOfGuid = TestUtils.CustomListAllRoutesForApp;

                CloudFoundry.CloudController.V2.Client.Base.Fakes.ShimAbstractRoutesEndpoint.AllInstances.RemoveAppFromRouteNullableOfGuidNullableOfGuid = TestUtils.CustomRemoveAppFromRoute;

                CloudFoundry.CloudController.V2.Client.Fakes.ShimPagedResponseCollection<ListAllOrganizationsResponse>.AllInstances.ResourcesGet = TestUtils.CustomListAllOrganizationsResponse;

                CloudFoundry.CloudController.V2.Client.Base.Fakes.ShimAbstractOrganizationsEndpoint.AllInstances.ListAllOrganizationsRequestOptions = TestUtils.CustomListAllOrganizations;

                CloudFoundry.CloudController.V2.Client.Fakes.ShimPagedResponseCollection<ListAllSpacesForOrganizationResponse>.AllInstances.ResourcesGet = TestUtils.CustomListAllSpacesForOrganizationResponse;

                CloudFoundry.CloudController.V2.Client.Base.Fakes.ShimAbstractOrganizationsEndpoint.AllInstances.ListAllSpacesForOrganizationNullableOfGuidRequestOptions = TestUtils.CustomListAllSpacesForOrganization;

                CloudFoundry.CloudController.V2.Client.Fakes.ShimPagedResponseCollection<ListAllRoutesResponse>.AllInstances.ResourcesGet = TestUtils.CustomListAllRoutesResponse;

                CloudFoundry.CloudController.V2.Client.Base.Fakes.ShimAbstractRoutesEndpoint.AllInstances.ListAllRoutesRequestOptions = TestUtils.CustomListAllRoutes;

                TestUtils.InitTestMetadata();

                UnbindRoute task = new UnbindRoute();
                task.CFUser = Settings.Default.User;
                task.CFPassword = Settings.Default.Password;
                task.CFServerUri = Settings.Default.ServerUri;
                task.CFSpace = "TestSpace";
                task.CFOrganization = "TestOrg";
                task.CFManifest = Settings.Default.CFManifest;

                task.BuildEngine = new FakeBuildEngine();
                Assert.IsTrue(task.Execute());
            }
        }
    }
}
