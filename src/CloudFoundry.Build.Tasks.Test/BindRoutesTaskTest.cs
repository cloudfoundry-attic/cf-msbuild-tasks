using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CloudFoundry.Build.Tasks.Test.Properties;
using Microsoft.QualityTools.Testing.Fakes;
using CloudFoundry.UAA;
using System.Threading.Tasks;
using CloudFoundry.CloudController.V2;
using CloudFoundry.CloudController.V2.Client.Data;
using System.Collections.Generic;

namespace CloudFoundry.Build.Tasks.Test
{
    [TestClass]
    public class BindRoutesTaskTest
    {
        [TestMethod]
        public void BindRoutes_Test()
        {
            using (ShimsContext.Create())
            {
                CloudFoundry.CloudController.V2.Client.Fakes.ShimCloudFoundryClient.AllInstances.LoginCloudCredentials = TestUtils.CustomLogin;

                CloudFoundry.CloudController.V2.Client.Base.Fakes.ShimAbstractAppsEndpoint.AllInstances.AssociateRouteWithAppNullableOfGuidNullableOfGuid = CustomAssociate;

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

                TestUtils.InitTestMetadata();

                BindRoutes task = new BindRoutes();
                task.CFManifest = Settings.Default.CFManifest;
                task.CFSpace = "TestSpace";
                task.CFOrganization = "TestOrg";
                task.CFUser = Settings.Default.User;
                task.CFPassword = Settings.Default.Password;
                task.CFSavedPassword = false;
                task.CFServerUri = Settings.Default.ServerUri;
                task.BuildEngine = new FakeBuildEngine();

                Assert.IsTrue(task.Execute());
            }
        }

        private Task<AssociateRouteWithAppResponse> CustomAssociate(CloudController.V2.Client.Base.AbstractAppsEndpoint arg1, Guid? arg2, Guid? arg3)
        {
            var task = Task.Run(() => { return new AssociateRouteWithAppResponse(); });
            return task;
        }

    }
}
