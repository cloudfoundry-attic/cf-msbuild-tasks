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
                Stack = Configuration.stack;
                AppName = Configuration.name;
                AppPath = Configuration.app_dir;
                Routes = Configuration.applications.Values.Select(o => o.url).ToArray();
                Memory = Configuration.memory;
                Instances = Configuration.instances;
                Autoscale = Utils.Serialize<Autoscale>(Configuration.autoscale);
                Disk = Configuration.disk;

                List<ProvisionedService> servicesList = new List<ProvisionedService>();
                foreach (var service in Configuration.services)
                {
                    servicesList.Add(new ProvisionedService() { Name = service.Key, Plan = service.Value.plan, Type = service.Value.type });
                }

                Services = Utils.Serialize<List<ProvisionedService>>(servicesList);
                PlacementZone = Configuration.placement_zone;
                SsoEnabled = Configuration.sso_enabled;

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
