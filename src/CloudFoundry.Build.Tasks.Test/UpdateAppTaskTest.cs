using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.QualityTools.Testing.Fakes;
using CloudFoundry.Build.Tasks.Test.Properties;

namespace CloudFoundry.Build.Tasks.Test
{
    [TestClass]
    public class UpdateAppTaskTest
    {
        [TestMethod]
        public void UpdateApp_Test()
        {
            using (ShimsContext.Create())
            {
                CloudFoundry.CloudController.V2.Client.Fakes.ShimCloudFoundryClient.AllInstances.LoginCloudCredentials = TestUtils.CustomLogin;

                CloudFoundry.CloudController.V2.Client.Base.Fakes.ShimAbstractAppsEndpoint.AllInstances.UpdateAppNullableOfGuidUpdateAppRequest = TestUtils.CustomUpdateApp;

                TestUtils.InitTestMetadata();

                UpdateApp task = new UpdateApp();
                task.CFAppGuid = Guid.NewGuid().ToString();
                task.CFUser = Settings.Default.User;
                task.CFPassword = Settings.Default.Password;
                task.CFServerUri = Settings.Default.ServerUri;
                task.BuildEngine = new FakeBuildEngine();

                Assert.IsTrue(task.Execute());
            }
        }
    }
}
