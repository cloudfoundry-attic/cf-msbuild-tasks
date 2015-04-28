using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CloudFoundry.Build.Tasks.IntegrationTests.Properties;
using System.Net;

namespace CloudFoundry.Build.Tasks.IntegrationTests
{
    [TestClass]
    public class TaskFlow_Test
    {
        [TestMethod]
        public void Flow_IntegrationTest()
        {
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
            task.CFAppName = "testIntegration";
            task.CFAppMemory = 512;
            task.CFAppInstances = 1;
            task.CFSpace = "TestSpace";
            task.CFOrganization = "TestOrg";
            task.CFStack = "win2012";
            task.CFEnvironmentJson = "{\"mykey\":\"abcd\",\"secondkey\":\"efgh\"}";

            task.BuildEngine = new FakeBuildEngine();
            task.Execute();

            PushApp pushTask = new PushApp();
            pushTask.CFUser = Settings.Default.User;
            pushTask.CFPassword = Settings.Default.Password;
            pushTask.CFServerUri = Settings.Default.ServerUri;
            pushTask.CFSkipSslValidation = true;
            pushTask.CFAppGuid = task.CFAppGuid;
            pushTask.CFAppPath = Settings.Default.AppPath;
            pushTask.CFStart = true;

            pushTask.BuildEngine = new FakeBuildEngine();
            
            pushTask.Execute();

            CreateRoutes routeTask = new CreateRoutes();
            routeTask.CFUser = Settings.Default.User;
            routeTask.CFPassword = Settings.Default.Password;
            routeTask.CFServerUri = Settings.Default.ServerUri;
            routeTask.CFSkipSslValidation = true;
            routeTask.CFRoutes = new string[1] { "testRoute.15.126.213.170.xip.io" };
            routeTask.CFSpace = "TestSpace";
            routeTask.CFOrganization = "TestOrg";

            routeTask.BuildEngine=new FakeBuildEngine();

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
            serviceTask.CFServiceName = "testService";
            serviceTask.CFServicePlan = "free";
            serviceTask.CFServiceType = "mysql";
            serviceTask.CFSpace = "TestSpace";
            serviceTask.CFOrganization = "TestOrg";
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

            if (CheckIfAppIsWorking(routeTask.CFRoutes[0], 3) == true)
            {

                DeleteApp delTask = new DeleteApp();
                delTask.CFUser = Settings.Default.User;
                delTask.CFPassword = Settings.Default.Password;
                delTask.CFServerUri = Settings.Default.ServerUri;
                delTask.CFSkipSslValidation = true;
                delTask.CFOrganization = "TestOrg";
                delTask.CFSpace = "TestSpace";
                delTask.CFAppName = "testIntegration";
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
            IPHostEntry info = null;
            for (int i = 0; i < retryCount; i++)
            {
                try
                {
                    info = System.Net.Dns.GetHostEntry(route);
                    break;
                }
                catch //If exception thrown then couldn't get response from address
                {
                    System.Threading.Thread.Sleep(7000);
                }
            }
            if (info == null)
            {
                return false;
            }
            return true;
        }

    }
}
