using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.QualityTools.Testing.Fakes;
using System.Threading.Tasks;
using CloudFoundry.UAA;
using CloudFoundry.Build.Tasks.Test.Properties;
using CloudFoundry.CloudController.V2.Client.Data;
using CloudFoundry.CloudController.V2.Client;

namespace CloudFoundry.Build.Tasks.Test
{
    [TestClass]
    public class BindServicesTaskTest
    {
        [TestMethod]
        public void BindServices_Test()
        {
            using (ShimsContext.Create())
            {
                CloudFoundry.CloudController.V2.Client.Fakes.ShimCloudFoundryClient.AllInstances.LoginCloudCredentials = TestUtils.CustomLogin;

                CloudFoundry.CloudController.V2.Client.Base.Fakes.ShimAbstractServiceBindingsEndpoint.AllInstances.CreateServiceBindingCreateServiceBindingRequest = CustomServiceBinding;

                TestUtils.InitTestMetadata();

                BindServices task = new BindServices();
                task.CFAppGuid = Guid.NewGuid().ToString();
                task.CFServicesGuids = new string[2] { Guid.NewGuid().ToString(), Guid.NewGuid().ToString() };
                task.CFUser = Settings.Default.User;
                task.CFPassword = Settings.Default.Password;
                task.CFServerUri = Settings.Default.ServerUri;
                task.BuildEngine = new FakeBuildEngine();

                task.Execute();

                Assert.AreEqual(task.CFBindingGuids.Length, 2);
            }

        }

        private Task<CreateServiceBindingResponse> CustomServiceBinding(CloudController.V2.Client.Base.AbstractServiceBindingsEndpoint arg1, CreateServiceBindingRequest arg2)
        {
            return Task.Factory.StartNew<CreateServiceBindingResponse>(() =>
            {
                return new CreateServiceBindingResponse() { EntityMetadata = new Metadata() };
            });
        }
    }
}
