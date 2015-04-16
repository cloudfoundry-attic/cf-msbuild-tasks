using CloudFoundry.Build.Tasks.Test.Properties;
using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudFoundry.Build.Tasks.Test
{
    [TestClass]
    public class RestartAppTaskTest
    {
        [TestMethod]
        public void RestartApp_Test()
        {
            using (ShimsContext.Create())
            {
                CloudFoundry.CloudController.V2.Client.Fakes.ShimCloudFoundryClient.AllInstances.LoginCloudCredentials = TestUtils.CustomLogin;

                CloudFoundry.CloudController.V2.Client.Base.Fakes.ShimAbstractAppsEndpoint.AllInstances.UpdateAppNullableOfGuidUpdateAppRequest = TestUtils.CustomUpdateApp;

                CloudFoundry.CloudController.V2.Client.Base.Fakes.ShimAbstractAppsEndpoint.AllInstances.GetAppSummaryNullableOfGuid = TestUtils.CustomGetAppSummary;

                CloudFoundry.CloudController.V2.Client.Fakes.ShimInfoEndpoint.AllInstances.GetV1Info = TestUtils.CustomGetV1Info;

                CloudFoundry.Logyard.Client.Fakes.ShimLogyardLog.AllInstances.StartLogStreamString = TestUtils.CustomStartLogStreamString;

                TestUtils.InitTestMetadata();

                RestartApp task = new RestartApp();
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
