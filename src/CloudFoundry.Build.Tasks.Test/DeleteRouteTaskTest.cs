using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.QualityTools.Testing.Fakes;
using CloudFoundry.Build.Tasks.Test.Properties;
using CloudFoundry.CloudController.V2.Client.Data;

namespace CloudFoundry.Build.Tasks.Test
{
    [TestClass]
    public class DeleteRouteTaskTest
    {
        [TestMethod]
        public void DeleteRoute_Test()
        {
            using (ShimsContext.Create())
            {
                CloudFoundry.CloudController.V2.Client.Fakes.ShimCloudFoundryClient.AllInstances.LoginCloudCredentials = TestUtils.CustomLogin;

                CloudFoundry.Manifests.Fakes.ShimManifestDiskRepository.ReadManifestString = TestUtils.CustomReadManifest;

                CloudFoundry.Manifests.Fakes.ShimManifest.AllInstances.Applications = TestUtils.CustomManifestApplications;

                CloudController.V2.Client.Base.Fakes.ShimAbstractDomainsDeprecatedEndpoint.AllInstances.ListAllDomainsDeprecated = TestUtils.CustomListAllDomains;

                CloudFoundry.CloudController.V2.Client.Fakes.ShimPagedResponseCollection<ListAllDomainsDeprecatedResponse>.AllInstances.ResourcesGet = TestUtils.CustomGetDomains;

                CloudFoundry.CloudController.V2.Client.Base.Fakes.ShimAbstractRoutesEndpoint.AllInstances.DeleteRouteNullableOfGuid = TestUtils.CustomDeleteRoute;

                CloudFoundry.CloudController.V2.Client.Fakes.ShimPagedResponseCollection<ListAllRoutesResponse>.AllInstances.ResourcesGet = TestUtils.CustomListAllRoutesResponse;

                CloudFoundry.CloudController.V2.Client.Base.Fakes.ShimAbstractRoutesEndpoint.AllInstances.ListAllRoutesRequestOptions = TestUtils.CustomListAllRoutes;

                TestUtils.InitTestMetadata();

                DeleteRoutes task = new DeleteRoutes();
                task.CFUser = Settings.Default.User;
                task.CFPassword = Settings.Default.Password;
                task.CFServerUri = Settings.Default.ServerUri;
                task.CFManifest = Settings.Default.CFManifest;
                task.CFOrganization = "TestOrg";
                task.CFSpace = "TestSpace";

                task.BuildEngine = new FakeBuildEngine();
                Assert.IsTrue(task.Execute());
            }
        }
    }
}
