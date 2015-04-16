using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.QualityTools.Testing.Fakes;
using CloudFoundry.CloudController.V2.Client.Data;
using CloudFoundry.Build.Tasks.Test.Properties;

namespace CloudFoundry.Build.Tasks.Test
{
    [TestClass]
    public class CreateServicesTaskTest
    {
        [TestMethod]
        public void CreateServices_Test()
        {
            using (ShimsContext.Create())
            {
                CloudFoundry.CloudController.V2.Client.Fakes.ShimCloudFoundryClient.AllInstances.LoginCloudCredentials = TestUtils.CustomLogin;

                CloudFoundry.CloudController.V2.Client.Base.Fakes.ShimAbstractSpacesEndpoint.AllInstances.ListAllSpacesRequestOptions = TestUtils.CustomListAllSpaces;

                CloudFoundry.CloudController.V2.Client.Fakes.ShimPagedResponseCollection<ListAllSpacesResponse>.AllInstances.ResourcesGet = TestUtils.CustomGetPaged;

                CloudFoundry.CloudController.V2.Client.Base.Fakes.ShimAbstractServicesEndpoint.AllInstances.ListAllServicesRequestOptions = TestUtils.CustomListAllServices;

                CloudFoundry.CloudController.V2.Client.Fakes.ShimPagedResponseCollection<ListAllServicesResponse>.AllInstances.ResourcesGet = TestUtils.CustomListAllServicesResponse;

                CloudFoundry.CloudController.V2.Client.Base.Fakes.ShimAbstractServicesEndpoint.AllInstances.ListAllServicePlansForServiceNullableOfGuid = TestUtils.CustomListServicePlans;

                CloudFoundry.CloudController.V2.Client.Fakes.ShimPagedResponseCollection<ListAllServicePlansForServiceResponse>.AllInstances.ResourcesGet = TestUtils.CustomListServicePlansResponse;

                CloudFoundry.CloudController.V2.Client.Base.Fakes.ShimAbstractServiceInstancesEndpoint.AllInstances.CreateServiceInstanceCreateServiceInstanceRequest = TestUtils.CustomCreateServiceInstance;

                TestUtils.InitTestMetadata();

                CreateServices task = new CreateServices();
                task.CFServices = @"<ArrayOfProvisionedService>   
   <ProvisionedService>
      <Name>service1</Name>
      <Plan>free</Plan>
      <Type>mysql</Type>
   </ProvisionedService>
   <ProvisionedService>
      <Name>service2</Name>
      <Plan>free</Plan>
      <Type>mssql2012</Type>
   </ProvisionedService>
</ArrayOfProvisionedService>";

                task.CFSpace = "TestSpace";
                task.CFUser = Settings.Default.User;
                task.CFPassword = Settings.Default.Password;
                task.CFServerUri = Settings.Default.ServerUri;
                task.BuildEngine = new FakeBuildEngine();

                task.Execute();

                Assert.AreEqual(task.ServicesGuids.Length, 2);
            }
        }
    }
}
