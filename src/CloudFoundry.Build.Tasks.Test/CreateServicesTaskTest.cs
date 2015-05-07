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

                CloudFoundry.CloudController.V2.Client.Base.Fakes.ShimAbstractServiceInstancesEndpoint.AllInstances.ListAllServiceInstances = TestUtils.CustomListAllServiceInstancesPlain;

                CloudFoundry.CloudController.V2.Client.Fakes.ShimPagedResponseCollection<ListAllServiceInstancesResponse>.AllInstances.ResourcesGet = TestUtils.CustomListAllServiceInstancesPlainResponse;

                CloudFoundry.CloudController.V2.Client.Fakes.ShimPagedResponseCollection<ListAllOrganizationsResponse>.AllInstances.ResourcesGet = TestUtils.CustomListAllOrganizationsResponse;

                CloudFoundry.CloudController.V2.Client.Base.Fakes.ShimAbstractOrganizationsEndpoint.AllInstances.ListAllOrganizationsRequestOptions = TestUtils.CustomListAllOrganizations;

                CloudFoundry.CloudController.V2.Client.Fakes.ShimPagedResponseCollection<ListAllSpacesForOrganizationResponse>.AllInstances.ResourcesGet = TestUtils.CustomListAllSpacesForOrganizationResponse;

                CloudFoundry.CloudController.V2.Client.Base.Fakes.ShimAbstractOrganizationsEndpoint.AllInstances.ListAllSpacesForOrganizationNullableOfGuidRequestOptions = TestUtils.CustomListAllSpacesForOrganization;


                TestUtils.InitTestMetadata();

                CreateServices task = new CreateServices();
                task.CFServices = @"service1,mysql,free;service2,mssql2012,free;";

                task.CFSpace = "TestSpace";
                task.CFOrganization = "TestOrg";
                task.CFUser = Settings.Default.User;
                task.CFPassword = Settings.Default.Password;
                task.CFServerUri = Settings.Default.ServerUri;
                task.BuildEngine = new FakeBuildEngine();

                task.Execute();

                Assert.AreEqual(2, task.CFServicesGuids.Length);
            }
        }
    }
}
