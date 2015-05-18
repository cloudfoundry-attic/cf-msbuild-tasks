using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CloudFoundry.Build.Tasks.IntegrationTests.Properties;
using System.Net;
using System.Reflection;
using System.IO;

namespace CloudFoundry.Build.Tasks.IntegrationTests
{
    [TestClass]
    [DeploymentItem(@"Assets")]
    public class TaskFlow_Test
    {
        [TestMethod]
        public void Flow_IntegrationTest()
        {
            string assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string appPath = Path.Combine(assemblyDirectory, "TestApp");

            LoginTask login = new LoginTask();
            login.BuildEngine = new FakeBuildEngine();
            login.CFUser = Settings.Default.User;
            login.CFPassword = Settings.Default.Password;
            login.CFServerUri = Settings.Default.ServerUri;
            login.CFSkipSslValidation = true;

            login.Execute();

            CreateApp task = new CreateApp();

            task.CFRefreshToken = login.CFRefreshToken;
            task.CFServerUri = Settings.Default.ServerUri;
            task.CFSkipSslValidation = true;
            task.CFAppName = Guid.NewGuid().ToString("N");
            task.CFAppMemory = 256;
            task.CFAppInstances = 1;
            task.CFSpace = Settings.Default.Space;
            task.CFOrganization = Settings.Default.Organization;
            task.CFStack = Settings.Default.Stack;
            task.CFEnvironmentJson = "{\"mykey\":\"abcd\",\"secondkey\":\"efgh\"}";

            task.BuildEngine = new FakeBuildEngine();
            task.Execute();

            PushApp pushTask = new PushApp();
            pushTask.CFUser = Settings.Default.User;
            pushTask.CFPassword = Settings.Default.Password;
            pushTask.CFServerUri = Settings.Default.ServerUri;
            pushTask.CFSkipSslValidation = true;
            pushTask.CFAppGuid = task.CFAppGuid;
            pushTask.CFAppPath = appPath;
            pushTask.CFStart = true;

            pushTask.BuildEngine = new FakeBuildEngine();

            pushTask.Execute();

            CreateRoutes routeTask = new CreateRoutes();
            routeTask.CFUser = Settings.Default.User;
            routeTask.CFPassword = Settings.Default.Password;
            routeTask.CFServerUri = Settings.Default.ServerUri;
            routeTask.CFSkipSslValidation = true;
            routeTask.CFRoutes = new string[1] { 
                string.Format(Settings.Default.Route, task.CFAppName)
            };
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

            bindTask.CFAppGuid = task.CFAppGuid;
            bindTask.CFRouteGuids = routeTask.CFRouteGuids;

            bindTask.Execute();

            CreateService serviceTask = new CreateService();
            serviceTask.CFUser = Settings.Default.User;
            serviceTask.CFPassword = Settings.Default.Password;
            serviceTask.CFServerUri = Settings.Default.ServerUri;
            serviceTask.BuildEngine = new FakeBuildEngine();
            serviceTask.CFSkipSslValidation = true;
            serviceTask.CFServiceName = Guid.NewGuid().ToString("N");
            serviceTask.CFServicePlan = Settings.Default.ServicePlan;
            serviceTask.CFServiceType = Settings.Default.ServiceType;
            serviceTask.CFSpace = Settings.Default.Space;
            serviceTask.CFOrganization = Settings.Default.Organization;
            serviceTask.Execute();

            BindServices bindServiceTask = new BindServices();
            bindServiceTask.CFUser = Settings.Default.User;
            bindServiceTask.CFPassword = Settings.Default.Password;
            bindServiceTask.CFServerUri = Settings.Default.ServerUri;
            bindServiceTask.BuildEngine = new FakeBuildEngine();
            bindServiceTask.CFSkipSslValidation = true;
            bindServiceTask.CFAppGuid = task.CFAppGuid;
            bindServiceTask.CFServicesGuids = new string[1] { serviceTask.CFServiceGuid };
            bindServiceTask.Execute();

            if (CheckIfAppIsWorking(routeTask.CFRoutes[0], 60) == true)
            {

                DeleteApp delTask = new DeleteApp();
                delTask.CFUser = Settings.Default.User;
                delTask.CFPassword = Settings.Default.Password;
                delTask.CFServerUri = Settings.Default.ServerUri;
                delTask.CFSkipSslValidation = true;
                delTask.CFSpace = Settings.Default.Space;
                delTask.CFOrganization = Settings.Default.Organization;
                delTask.CFAppName = task.CFAppName;
                delTask.CFDeleteServices = true;
                delTask.CFDeleteRoutes = true;
                delTask.BuildEngine = new FakeBuildEngine();

                Assert.IsTrue(delTask.Execute());
            }
            else
            {
                Assert.Fail("Application is not working");
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