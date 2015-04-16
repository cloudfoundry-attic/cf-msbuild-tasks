using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.QualityTools.Testing.Fakes;
using CloudFoundry.CloudController.V2.Client.Data;
using CloudFoundry.Build.Tasks.Test.Properties;

namespace CloudFoundry.Build.Tasks.Test
{
    [TestClass]
    public class DeleteAppTaskTest
    {
        [TestMethod]
        public void DeleteApp_Test()
        {
            using (ShimsContext.Create())
            {
                CloudFoundry.CloudController.V2.Client.Fakes.ShimCloudFoundryClient.AllInstances.LoginCloudCredentials = TestUtils.CustomLogin;

                CloudFoundry.CloudController.V2.Client.Base.Fakes.ShimAbstractSpacesEndpoint.AllInstances.ListAllSpacesRequestOptions = TestUtils.CustomListAllSpaces;

                CloudFoundry.CloudController.V2.Client.Fakes.ShimPagedResponseCollection<ListAllSpacesResponse>.AllInstances.ResourcesGet = TestUtils.CustomGetPaged;

                CloudFoundry.CloudController.V2.Client.Base.Fakes.ShimAbstractAppsEndpoint.AllInstances.DeleteAppNullableOfGuid = TestUtils.CustomDeleteApp;

                CloudFoundry.CloudController.V2.Client.Fakes.ShimPagedResponseCollection<ListAllAppsForSpaceResponse>.AllInstances.ResourcesGet = TestUtils.CusomListAllAppsForSpacePagedResponse;

                CloudFoundry.CloudController.V2.Client.Base.Fakes.ShimAbstractSpacesEndpoint.AllInstances.ListAllAppsForSpaceNullableOfGuidRequestOptions = TestUtils.CustomListAllAppsForSpace;

                CloudFoundry.CloudController.V2.Client.Fakes.ShimPagedResponseCollection<ListAllRoutesForAppResponse>.AllInstances.ResourcesGet = TestUtils.CustomListAllRoutesForAppResponse;

                CloudFoundry.CloudController.V2.Client.Base.Fakes.ShimAbstractAppsEndpoint.AllInstances.ListAllRoutesForAppNullableOfGuid = TestUtils.CustomListAllRoutesForApp;

                CloudFoundry.CloudController.V2.Client.Base.Fakes.ShimAbstractRoutesEndpoint.AllInstances.DeleteRouteNullableOfGuid = TestUtils.CustomDeleteRoute;

                CloudFoundry.CloudController.V2.Client.Base.Fakes.ShimAbstractAppsEndpoint.AllInstances.ListAllServiceBindingsForAppNullableOfGuid = TestUtils.CustomListAllServiceBindingsForApp;

                CloudFoundry.CloudController.V2.Client.Fakes.ShimPagedResponseCollection<ListAllServiceBindingsForAppResponse>.AllInstances.ResourcesGet = TestUtils.CustomListAllServiceBindingsForAppResponse;

                CloudFoundry.CloudController.V2.Client.Base.Fakes.ShimAbstractServiceBindingsEndpoint.AllInstances.DeleteServiceBindingNullableOfGuid = TestUtils.CustomDeleteServiceBinding;

                CloudFoundry.CloudController.V2.Client.Base.Fakes.ShimAbstractServiceInstancesEndpoint.AllInstances.DeleteServiceInstanceNullableOfGuid = TestUtils.CustomDeleteServiceInstance;

                TestUtils.InitTestMetadata();

                DeleteApp task = new DeleteApp();
                task.CFUser = Settings.Default.User;
                task.CFPassword = Settings.Default.Password;
                task.CFServerUri = Settings.Default.ServerUri;
                task.CFSpace = "TestSpace";
                task.CFAppName = "testApp";
                task.CFDeleteRoutes = true;
                task.CFDeleteServices = true;

                task.BuildEngine = new FakeBuildEngine();
                Assert.IsTrue(task.Execute());
            }
        }
    }
}
