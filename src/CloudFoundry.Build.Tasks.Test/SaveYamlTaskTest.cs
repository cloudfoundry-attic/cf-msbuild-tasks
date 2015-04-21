using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace CloudFoundry.Build.Tasks.Test
{
    [TestClass]
    public class SaveYamlTaskTest
    {
        [TestMethod]
        public void SaveYaml_Test()
        {
            string tempFile=Path.GetTempFileName();

            SaveYaml task = new SaveYaml();
            task.BuildEngine = new FakeBuildEngine();
            task.CFAppName = "test";
            task.CFAppPath=@"C:\test\";
            task.CFConfigurationFile=tempFile;
            task.CFStack="win2012r2";
            task.CFRoute = "test.1.2.3.4.xip.io";
            task.CFInstancesNumber = 1;
            task.CFAppMemory = 512;

            Assert.IsTrue(task.Execute());

            File.Delete(tempFile);
        }
    }
}
