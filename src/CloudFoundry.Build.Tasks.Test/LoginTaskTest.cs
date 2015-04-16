using CloudFoundry.Build.Tasks;
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
    public class LoginTaskTest
    {
        [TestMethod]
        public void LoginFail_Test()
        {
            using (ShimsContext.Create())
            {
                CloudFoundry.CloudController.V2.Client.Fakes.ShimCloudFoundryClient.AllInstances.LoginCloudCredentials = TestUtils.CustomLogin;
                
                LoginTask login = new LoginTask();
                login.CFUser = Settings.Default.User;
                login.CFPassword = Settings.Default.Password;
                login.CFServerUri = Settings.Default.ServerUri;

                login.BuildEngine = new FakeBuildEngine();

                Assert.IsFalse(login.Execute());
            }
        }
    }
}
