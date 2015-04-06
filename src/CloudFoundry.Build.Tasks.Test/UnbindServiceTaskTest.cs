using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CloudFoundry.Build.Tasks.Test.Properties;
using CloudFoundry.CloudController.V2.Client.Data;
using Microsoft.QualityTools.Testing.Fakes;

namespace CloudFoundry.Build.Tasks.Test
{
    [TestClass]
    public class UnbindServiceTaskTest
    {
        [TestMethod]
        public void UnbindService_Test()
        {
            using (ShimsContext.Create())
            {
                CloudFoundry.CloudController.V2.Client.Fakes.ShimCloudFoundryClient.AllInstances.LoginCloudCredentials = TestUtils.CustomLogin;

                CloudFoundry.CloudController.V2.Client.Base.Fakes.ShimAbstractSpacesEndpoint.AllInstances.ListAllSpacesRequestOptions = TestUtils.CustomListAllSpaces;

                CloudFoundry.CloudController.V2.Client.Fakes.ShimPagedResponseCollection<ListAllSpacesResponse>.AllInstances.ResourcesGet = TestUtils.CustomGetPaged;

                CloudFoundry.CloudController.V2.Client.Fakes.ShimPagedResponseCollection<ListAllAppsForSpaceResponse>.AllInstances.ResourcesGet = TestUtils.CusomListAllAppsForSpacePagedResponse;

                CloudFoundry.CloudController.V2.Client.Base.Fakes.ShimAbstractSpacesEndpoint.AllInstances.ListAllAppsForSpaceNullableOfGuidRequestOptions = TestUtils.CustomListAllAppsForSpace;

                CloudFoundry.CloudController.V2.Client.Base.Fakes.ShimAbstractAppsEndpoint.AllInstances.ListAllServiceBindingsForAppNullableOfGuid = TestUtils.CustomListAllServiceBindingsForApp;

                CloudFoundry.CloudController.V2.Client.Fakes.ShimPagedResponseCollection<ListAllServiceBindingsForAppResponse>.AllInstances.ResourcesGet = TestUtils.CustomListAllServiceBindingsForAppResponse;

                CloudFoundry.CloudController.V2.Client.Base.Fakes.ShimAbstractServiceBindingsEndpoint.AllInstances.DeleteServiceBindingNullableOfGuid = TestUtils.CustomDeleteServiceBinding;

                CloudFoundry.CloudController.V2.Client.Base.Fakes.ShimAbstractSpacesEndpoint.AllInstances.ListAllServiceInstancesForSpaceNullableOfGuidRequestOptions = TestUtils.CustomListAllServiceInstances;

                CloudFoundry.CloudController.V2.Client.Fakes.ShimPagedResponseCollection<ListAllServiceInstancesForSpaceResponse>.AllInstances.ResourcesGet = TestUtils.CustomListAllServiceInstancesResponse;

                TestUtils.InitTestMetadata();

                UnbindService task = new UnbindService();
                task.User = Settings.Default.User;
                task.Password = Settings.Default.Password;
                task.ServerUri = Settings.Default.ServerUri;
                task.Space = "TestSpace";
                task.AppName = "testApp";
                task.ServiceName = "service1";
                task.BuildEngine = new FakeBuildEngine();
                Assert.IsTrue(task.Execute());
            }
        }
    }
}
