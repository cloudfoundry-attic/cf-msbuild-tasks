using Microsoft.Build.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudFoundry.Build.Tasks
{
    public class LoadYaml:ITask 
    {
        private IBuildEngine buildEngine;
        private ITaskHost taskHost;
        private Microsoft.Build.Utilities.TaskLoggingHelper logger;
        public IBuildEngine BuildEngine
        {
            get
            { return buildEngine; }
            set
            { buildEngine = value; }
        }

        [Required]
        public string ConfigurationFile { get; set; }

        [Output]
        public string Stack { get; set; }

        [Output]
        public string AppName { get; set; }

        [Output]
        public string AppPath { get; set; }

        [Output]
        public string[] Routes { get; set; }

        [Output]
        public int Memory { get; set; }

        [Output]
        public int Instances { get; set; }

        [Output]
        public string Autoscale { get; set; }

        [Output]
        public int Disk { get; set; }

        [Output]
        public string Services { get; set; }

        [Output]
        public string PlacementZone { get; set; }

        [Output]
        public string SsoEnabled { get; set; }

        private PushProperties Configuration { get; set; }

        public bool Execute()
        {
            logger = new Microsoft.Build.Utilities.TaskLoggingHelper(this);
            logger.LogMessage("Loading configuration from {0}", ConfigurationFile);
            try
            {
                Configuration = Utils.DeserializeFromFile(ConfigurationFile);
                Stack = Configuration.Stack;
                AppName = Configuration.Name;
                AppPath = Configuration.AppDir;
                Routes = Configuration.Applications.Values.Select(o => o.Url).ToArray();
                Memory = Configuration.Memory;
                Instances = Configuration.Instances;
                Autoscale = Utils.Serialize<Autoscale>(Configuration.AutoscaleInfo);
                Disk = Configuration.Disk;

                List<ProvisionedService> servicesList = new List<ProvisionedService>();
                foreach (var service in Configuration.Services)
                {
                    servicesList.Add(new ProvisionedService() { Name = service.Key, Plan = service.Value.Plan, Type = service.Value.Type });
                }

                Services = Utils.Serialize<List<ProvisionedService>>(servicesList);
                PlacementZone = Configuration.PlacementZone;
                SsoEnabled = Configuration.SsoEnabled;

                //logger.LogMessage("Autoscale settings: {0}", Autoscale);
                //logger.LogMessage("Services configuration: {0}", Services);
                logger.LogMessage("Configuration loaded");
            }
            catch (Exception ex)
            {
                logger.LogErrorFromException(ex);
                throw;
            }

            return true;
        }

        public ITaskHost HostObject
        {
            get
            { return taskHost; }
            set
            { taskHost = value; }
        }
    }
}
