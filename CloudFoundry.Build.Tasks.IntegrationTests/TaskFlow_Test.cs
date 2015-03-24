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
            CreateApp task = new CreateApp();

            task.User = Settings.Default.User;
            task.Password = Settings.Default.Password;
            task.ServerUri = Settings.Default.ServerUri;

            task.Name = "testIntegration";
            task.Memory = 512;
            task.Instances = 1;
            task.Space = "TestSpace";
            task.Stack = "win2012";

            task.BuildEngine = new FakeBuildEngine();
            task.Execute();

            PushApp pushTask = new PushApp();
            pushTask.User = Settings.Default.User;
            pushTask.Password = Settings.Default.Password;
            pushTask.ServerUri = Settings.Default.ServerUri;

            pushTask.AppGuid = task.AppGuid;
            pushTask.AppPath = Settings.Default.AppPath;
            pushTask.Start = true;

            pushTask.BuildEngine = new FakeBuildEngine();
            
            pushTask.Execute();

            CreateRoutes routeTask = new CreateRoutes();
            routeTask.User = Settings.Default.User;
            routeTask.Password = Settings.Default.Password;
            routeTask.ServerUri = Settings.Default.ServerUri;

            routeTask.Routes = new string[1] { "testRoute.15.126.213.170.xip.io" };
            routeTask.Space = "TestSpace";
            routeTask.BuildEngine=new FakeBuildEngine();

            routeTask.Execute();

            BindRoutes bindTask = new BindRoutes();
            bindTask.User = Settings.Default.User;
            bindTask.Password = Settings.Default.Password;
            bindTask.ServerUri = Settings.Default.ServerUri;
            bindTask.BuildEngine = new FakeBuildEngine();


            bindTask.AppGuid = task.AppGuid;
            bindTask.RouteGuids = routeTask.RouteGuids;

            bindTask.Execute();
           
            CreateService serviceTask = new CreateService();
            serviceTask.User = Settings.Default.User;
            serviceTask.Password = Settings.Default.Password;
            serviceTask.ServerUri = Settings.Default.ServerUri;
            serviceTask.BuildEngine = new FakeBuildEngine();

            serviceTask.Name = "testService";
            serviceTask.ServicePlan = "free";
            serviceTask.ServiceType = "mysql";
            serviceTask.Space = "TestSpace";

            serviceTask.Execute();

            BindServices bindServiceTask = new BindServices();
            bindServiceTask.User = Settings.Default.User;
            bindServiceTask.Password = Settings.Default.Password;
            bindServiceTask.ServerUri = Settings.Default.ServerUri;
            bindServiceTask.BuildEngine = new FakeBuildEngine();

            bindServiceTask.AppGuid = task.AppGuid;
            bindServiceTask.ServicesGuids = new string[1] { serviceTask.ServiceGuid };
            bindServiceTask.Execute();

            if (CheckIfAppIsWorking(routeTask.Routes[0], 3) == true)
            {

                DeleteApp delTask = new DeleteApp();
                delTask.User = Settings.Default.User;
                delTask.Password = Settings.Default.Password;
                delTask.ServerUri = Settings.Default.ServerUri;

                delTask.Space = "TestSpace";
                delTask.AppName = "testIntegration";
                delTask.DeleteServices = true;
                delTask.DeleteRoutes = true;
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
