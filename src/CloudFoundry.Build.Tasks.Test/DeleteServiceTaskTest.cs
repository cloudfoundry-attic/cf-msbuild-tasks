using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.QualityTools.Testing.Fakes;
using CloudFoundry.CloudController.V2.Client.Data;
using CloudFoundry.Build.Tasks.Test.Properties;

namespace CloudFoundry.Build.Tasks.Test
{
    [TestClass]
    public class DeleteServiceTaskTest
    {
        [TestMethod]
        public void DeleteService_Test()
        {
            using (ShimsContext.Create())
            {
                CloudFoundry.CloudController.V2.Client.Fakes.ShimCloudFoundryClient.AllInstances.LoginCloudCredentials = TestUtils.CustomLogin;

                CloudFoundry.CloudController.V2.Client.Base.Fakes.ShimAbstractSpacesEndpoint.AllInstances.ListAllSpacesRequestOptions = TestUtils.CustomListAllSpaces;

                CloudFoundry.CloudController.V2.Client.Fakes.ShimPagedResponseCollection<ListAllSpacesResponse>.AllInstances.ResourcesGet = TestUtils.CustomGetPaged;

                CloudFoundry.CloudController.V2.Client.Base.Fakes.ShimAbstractSpacesEndpoint.AllInstances.ListAllServiceInstancesForSpaceNullableOfGuidRequestOptions = TestUtils.CustomListAllServiceInstances;

                CloudFoundry.CloudController.V2.Client.Fakes.ShimPagedResponseCollection<ListAllServiceInstancesForSpaceResponse>.AllInstances.ResourcesGet = TestUtils.CustomListAllServiceInstancesResponse;

                CloudFoundry.CloudController.V2.Client.Base.Fakes.ShimAbstractServiceInstancesEndpoint.AllInstances.DeleteServiceInstanceNullableOfGuid = TestUtils.CustomDeleteServiceInstance;

                TestUtils.InitTestMetadata();

                DeleteService task = new DeleteService();
                task.CFUser = Settings.Default.User;
                task.CFPassword = Settings.Default.Password;
                task.CFServerUri = Settings.Default.ServerUri;
                task.CFSpace = "TestSpace";
                task.CFServiceName = "service1";

                task.BuildEngine = new FakeBuildEngine();
                Assert.IsTrue(task.Execute());

            }
        }

    }
}
