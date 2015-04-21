using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.QualityTools.Testing.Fakes;
using System.IO;

namespace CloudFoundry.Build.Tasks.Test
{
    [TestClass]
    public class LoadYamlTaskTest
    {
        [TestMethod]
        public void LoadYaml_Test()
        {

            string tempFile = Path.GetTempFileName();

            File.WriteAllText(tempFile, @"app-dir: C:/test/
applications:
  C:/test/:
    name: testapp
    url: test1.1.2.3.4.xip.io;test2.1.2.3.4.xip.io
autoscale:
  cpu:
    max: 80
    min: 20
  enabled: no
  instances:
    max: 2
    min: 1
disk: 1024
instances: 1
memory: 1024
name: testapp
placement-zone: default
services:
  service1:
    plan: free
    type: mysql
sso-enabled: no
stack: win2012r2
");

            LoadYaml task = new LoadYaml();
            task.CFConfigurationFile = tempFile;
            task.BuildEngine = new FakeBuildEngine();

            Assert.IsTrue(task.Execute());
            File.Delete(tempFile);
        }
    }
}
