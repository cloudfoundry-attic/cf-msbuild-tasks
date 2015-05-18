using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CloudFoundry.Build.Tasks.IntegrationTests
{
    [TestClass]
    [Ignore]
    public class LoadYamlTest
    {
        [TestMethod]
        public void LoadYaml_Test()
        {
            string pathToManifest = @"C:\test\manifest.yaml";

            if (File.Exists(pathToManifest))
            {
                LoadYaml task = new LoadYaml();
                task.BuildEngine = new FakeBuildEngine();
                task.CFConfigurationFile = pathToManifest;
                Assert.IsTrue(task.Execute());
            }
            else
            {
                throw new Exception("Manifest file not found, test not executed");
            }
        }
    }
}
