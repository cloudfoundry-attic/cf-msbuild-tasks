using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CloudFoundry.Build.Tasks.IntegrationTests.Properties;
using System.Net;
using System.Reflection;
using System.IO;
using Microsoft.Build.Utilities;
using System.Globalization;

namespace CloudFoundry.Build.Tasks.IntegrationTests
{
    [TestClass]
    [DeploymentItem(@"Assets")]
    public class TaskFlow_Test
    {
        [TestMethod]
        public void PhpApp_IntegrationTest()
        {
            string assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string appPath = Path.Combine(assemblyDirectory, "PhpApp");
            string manifest = Path.Combine(assemblyDirectory, "testmanifest.yml");
            string phpManifest = Path.Combine(assemblyDirectory, "phpmanifest.yml");

            string guid = Guid.NewGuid().ToString();
            string host = string.Format(Settings.Default.Host, guid);

            string man = File.ReadAllText(manifest);

            man = string.Format(CultureInfo.InvariantCulture, man, guid, host, Settings.Default.Domain, appPath, Settings.Default.LinuxStack);
            File.WriteAllText(phpManifest, man);

            Assert.IsTrue(RunTasks(phpManifest, host, appPath));
        }

        [TestMethod]
        public void AspApp_IntegrationTest()
        {
            string assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string appPath = Path.Combine(assemblyDirectory, "TestApp");
            string manifest = Path.Combine(assemblyDirectory, "testmanifest.yml");
            string aspManifest = Path.Combine(assemblyDirectory, "aspmanifest.yml"); 

            string guid = Guid.NewGuid().ToString();
            string host = string.Format(Settings.Default.Host, guid);

            string man = File.ReadAllText(manifest);

            man = string.Format(CultureInfo.InvariantCulture, man, guid, host, Settings.Default.Domain, appPath, Settings.Default.WindowsStack);

            File.WriteAllText(aspManifest, man);

            Assert.IsTrue(RunTasks(aspManifest, host, appPath));
        }

        private bool RunTasks(string manifest, string host, string appPath)
        {
            LoginTask login = new LoginTask();
            login.BuildEngine = new FakeBuildEngine();
            login.CFUser = Settings.Default.User;
            login.CFPassword = Settings.Default.Password;
            login.CFServerUri = Settings.Default.ServerUri;
            login.CFManifest = manifest;
            login.CFSkipSslValidation = true;

            login.Execute();

            CreateApp task = new CreateApp();

            task.CFRefreshToken = login.CFRefreshToken;
            task.CFServerUri = Settings.Default.ServerUri;
            task.CFSkipSslValidation = true;
            task.CFSpace = Settings.Default.Space;
            task.CFOrganization = Settings.Default.Organization;
            task.CFManifest = manifest;
            task.BuildEngine = new FakeBuildEngine();
            task.Execute();

            PushApp pushTask = new PushApp();
            pushTask.CFUser = Settings.Default.User;
            pushTask.CFPassword = Settings.Default.Password;
            pushTask.CFServerUri = Settings.Default.ServerUri;
            pushTask.CFSkipSslValidation = true;
            pushTask.CFManifest = manifest;
            pushTask.CFOrganization = Settings.Default.Organization;
            pushTask.CFSpace = Settings.Default.Space;
            pushTask.CFAppPath = appPath;

            pushTask.BuildEngine = new FakeBuildEngine();

            pushTask.Execute();

            CreateRoutes routeTask = new CreateRoutes();
            routeTask.CFUser = Settings.Default.User;
            routeTask.CFPassword = Settings.Default.Password;
            routeTask.CFServerUri = Settings.Default.ServerUri;
            routeTask.CFSkipSslValidation = true;
            routeTask.CFManifest = manifest;
            routeTask.CFSpace = Settings.Default.Space;
            routeTask.CFOrganization = Settings.Default.Organization;

            routeTask.BuildEngine = new FakeBuildEngine();

            routeTask.Execute();

            BindRoutes bindTask = new BindRoutes();
            bindTask.CFUser = Settings.Default.User;
            bindTask.CFPassword = Settings.Default.Password;
            bindTask.CFServerUri = Settings.Default.ServerUri;
            bindTask.BuildEngine = new FakeBuildEngine();
            bindTask.CFSkipSslValidation = true;
            bindTask.CFManifest = manifest;
            bindTask.CFOrganization = Settings.Default.Organization;
            bindTask.CFSpace = Settings.Default.Space;

            bindTask.Execute();

            BindServices bindServiceTask = new BindServices();
            bindServiceTask.CFUser = Settings.Default.User;
            bindServiceTask.CFPassword = Settings.Default.Password;
            bindServiceTask.CFServerUri = Settings.Default.ServerUri;
            bindServiceTask.BuildEngine = new FakeBuildEngine();
            bindServiceTask.CFSkipSslValidation = true;
            bindServiceTask.CFManifest = manifest;
            bindServiceTask.CFOrganization = Settings.Default.Organization;
            bindServiceTask.CFSpace = Settings.Default.Space;
            bindServiceTask.Execute();

            RestartApp restartTask = new RestartApp();
            restartTask.BuildEngine = new FakeBuildEngine();
            restartTask.CFUser = Settings.Default.User;
            restartTask.CFPassword = Settings.Default.Password;
            restartTask.CFServerUri = Settings.Default.ServerUri;
            restartTask.CFManifest = manifest;
            restartTask.CFSkipSslValidation = true;
            restartTask.CFOrganization = Settings.Default.Organization;
            restartTask.CFSpace = Settings.Default.Space;
            restartTask.Execute();

            if (CheckIfAppIsWorking(string.Format(CultureInfo.InvariantCulture, "{0}.{1}", host, Settings.Default.Domain), 60) == true)
            {
                DeleteApp delTask = new DeleteApp();
                delTask.CFUser = Settings.Default.User;
                delTask.CFPassword = Settings.Default.Password;
                delTask.CFServerUri = Settings.Default.ServerUri;
                delTask.CFSkipSslValidation = true;
                delTask.CFSpace = Settings.Default.Space;
                delTask.CFOrganization = Settings.Default.Organization;
                delTask.CFManifest = manifest;
                delTask.CFDeleteServices = true;
                delTask.CFDeleteRoutes = true;
                delTask.BuildEngine = new FakeBuildEngine();

                return delTask.Execute();
            }
            else
            {
                return false;
            }
        }

        private bool CheckIfAppIsWorking(string route, int retryCount)
        {
            for (int i = 0; i < retryCount; i++)
            {
                try
                {
                    WebRequest request = WebRequest.Create(string.Format("http://{0}", route));
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        return true;
                    }
                    else
                    {
                        System.Threading.Thread.Sleep(5000);
                    }
                }
                catch //If exception thrown then couldn't get response from address
                {
                    System.Threading.Thread.Sleep(5000);
                }
            }
            return false;
        }
    }
}