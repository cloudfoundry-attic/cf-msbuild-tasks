using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.QualityTools.Testing.Fakes;
using CloudFoundry.Build.Tasks.Test.Properties;
using CloudFoundry.UAA;
using System.Threading.Tasks;
using CloudFoundry.CloudController.V2.Client.Data;
using System.Collections.Generic;
using CloudFoundry.CloudController.V2;

namespace CloudFoundry.Build.Tasks.Test
{
    [TestClass]
    public class CreateRoutesTaskTest
    {
        [TestMethod]
        public void CreateRoutes_Test()
        {
            using (ShimsContext.Create())
            {
                CloudFoundry.CloudController.V2.Client.Fakes.ShimCloudFoundryClient.AllInstances.LoginCloudCredentials = TestUtils.CustomLogin;

                CloudFoundry.CloudController.V2.Client.Base.Fakes.ShimAbstractSpacesEndpoint.AllInstances.ListAllSpacesRequestOptions = TestUtils.CustomListAllSpaces;

                CloudFoundry.CloudController.V2.Client.Fakes.ShimPagedResponseCollection<ListAllSpacesResponse>.AllInstances.ResourcesGet = TestUtils.CustomGetPaged;

                CloudController.V2.Client.Base.Fakes.ShimAbstractDomainsDeprecatedEndpoint.AllInstances.ListAllDomainsDeprecated = TestUtils.CustomListAllDomains;

                CloudFoundry.CloudController.V2.Client.Fakes.ShimPagedResponseCollection<ListAllDomainsDeprecatedResponse>.AllInstances.ResourcesGet = TestUtils.CustomGetDomains;

                CloudFoundry.CloudController.V2.Client.Base.Fakes.ShimAbstractRoutesEndpoint.AllInstances.CreateRouteCreateRouteRequest = TestUtils.CustomCreateRoute;

                TestUtils.InitTestMetadata();

                CreateRoutes task = new CreateRoutes();
                task.CFRoutes = new string[2] { "test.domain.com" ,"test2.domain.com" };
                task.CFSpace = "TestSpace";
                task.CFUser = Settings.Default.User;
                task.CFPassword = Settings.Default.Password;
                task.CFServerUri = Settings.Default.ServerUri;
                task.BuildEngine = new FakeBuildEngine();

                task.Execute();

                Assert.AreEqual(task.CFRouteGuids.Length, 2);
            }

        }
    }
}
