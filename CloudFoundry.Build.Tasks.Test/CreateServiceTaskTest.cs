using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.QualityTools.Testing.Fakes;
using CloudFoundry.CloudController.V2.Client.Data;
using CloudFoundry.Build.Tasks.Test.Properties;
using System.Collections.Generic;
using CloudFoundry.CloudController.V2;
using System.Threading.Tasks;
using CloudFoundry.UAA;

namespace CloudFoundry.Build.Tasks.Test
{
    [TestClass]
    public class CreateServiceTaskTest
    {
        [TestMethod]
        public void CreateService_Test()
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

                CreateService task = new CreateService();
                task.Name = "testService";
                task.ServicePlan = "myPlan";
                task.ServiceType = "myType";
                task.Space = "TestSpace";
                task.User = Settings.Default.User;
                task.Password = Settings.Default.Password;
                task.ServerUri = Settings.Default.ServerUri;
                task.BuildEngine = new FakeBuildEngine();

                task.Execute();

                Assert.IsNotNull(task.ServiceGuid);
            }
        }

      
    }
}
