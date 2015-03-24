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

                CloudFoundry.CloudController.V2.Client.Base.Fakes.ShimAbstractSpacesEndpoint.AllInstances.ListAllSpacesRequestOptions = TestUtils.CustomListAllSpaces;

                CloudFoundry.CloudController.V2.Client.Fakes.ShimPagedResponseCollection<ListAllSpacesResponse>.AllInstances.ResourcesGet = TestUtils.CustomGetPaged;

                CloudFoundry.CloudController.V2.Client.Fakes.ShimPagedResponseCollection<ListAllAppsForSpaceResponse>.AllInstances.ResourcesGet = TestUtils.CusomListAllAppsForSpacePagedResponse;

                CloudController.V2.Client.Base.Fakes.ShimAbstractDomainsDeprecatedEndpoint.AllInstances.ListAllDomainsDeprecated = TestUtils.CustomListAllDomains;

                CloudFoundry.CloudController.V2.Client.Fakes.ShimPagedResponseCollection<ListAllDomainsDeprecatedResponse>.AllInstances.ResourcesGet = TestUtils.CustomGetDomains;

                CloudFoundry.CloudController.V2.Client.Base.Fakes.ShimAbstractSpacesEndpoint.AllInstances.ListAllAppsForSpaceNullableOfGuidRequestOptions = TestUtils.CustomListAllAppsForSpace;

                CloudFoundry.CloudController.V2.Client.Fakes.ShimPagedResponseCollection<ListAllRoutesForAppResponse>.AllInstances.ResourcesGet = TestUtils.CustomListAllRoutesForAppResponse;

                CloudFoundry.CloudController.V2.Client.Base.Fakes.ShimAbstractAppsEndpoint.AllInstances.ListAllRoutesForAppNullableOfGuid = TestUtils.CustomListAllRoutesForApp;

                CloudFoundry.CloudController.V2.Client.Base.Fakes.ShimAbstractRoutesEndpoint.AllInstances.RemoveAppFromRouteNullableOfGuidNullableOfGuid = TestUtils.CustomRemoveAppFromRoute;

                TestUtils.InitTestMetadata();

                UnbindRoute task = new UnbindRoute();
                task.User = Settings.Default.User;
                task.Password = Settings.Default.Password;
                task.ServerUri = Settings.Default.ServerUri;
                task.Space = "TestSpace";
                task.AppName = "testApp";
                task.Route = "test.domain.com";

                task.BuildEngine = new FakeBuildEngine();
                Assert.IsTrue(task.Execute());
            }
        }
    }
}
