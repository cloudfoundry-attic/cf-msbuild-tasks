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
        private TaskLogger logger;
        public IBuildEngine BuildEngine
        {
            get
            { return buildEngine; }
            set
            { buildEngine = value; }
        }

        [Required]
        public string CFConfigurationFile { get; set; }

        [Output]
        public string CFStack { get; set; }

        [Output]
        public string CFAppName { get; set; }

        [Output]
        public string CFAppPath { get; set; }

        [Output]
        public string CFRoutes { get; set; }

        [Output]
        public int CFAppMemory { get; set; }

        [Output]
        public int CFAppInstances { get; set; }

        [Output]
        public string CFAutoscale { get; set; }

        [Output]
        public int CFDisk { get; set; }

        [Output]
        public string CFServices { get; set; }

        [Output]
        public string CFPlacementZone { get; set; }

        [Output]
        public string CFSsoEnabled { get; set; }

        private PushProperties Configuration { get; set; }

        public bool Execute()
        {
            logger = new TaskLogger(this);
            logger.LogMessage("Loading configuration from {0}", CFConfigurationFile);
            try
            {
                Configuration = Utils.DeserializeFromFile(CFConfigurationFile);
                CFStack = Configuration.Stack;
                CFAppName = Configuration.Name;
                CFAppPath = Configuration.AppDir;

                List<string> urlList = new List<string>();
                foreach (var app in Configuration.Applications)
                {
                    if (app.Value.Url.Contains(";"))
                    {
                        string[] appurls = app.Value.Url.Split(';');
                        urlList.AddRange(appurls);
                    }
                    else
                    {
                        urlList.Add(app.Value.Url);
                    }
                }
                foreach (string s in urlList)
                {
                    logger.LogMessage("Loaded url {0}", s);
                }

                CFRoutes = string.Join(";",urlList);
                
                CFAppMemory = Configuration.Memory;
                CFAppInstances = Configuration.Instances;
                CFAutoscale = Utils.Serialize<Autoscale>(Configuration.AutoscaleInfo);
                CFDisk = Configuration.Disk;

                if (Configuration.Services != null)
                {
                    List<ProvisionedService> servicesList = new List<ProvisionedService>();
                    foreach (var service in Configuration.Services)
                    {
                        servicesList.Add(new ProvisionedService() { Name = service.Key, Plan = service.Value.Plan, Type = service.Value.Type });
                    }

                    CFServices = Utils.Serialize<List<ProvisionedService>>(servicesList);
                }
                CFPlacementZone = Configuration.PlacementZone;
                CFSsoEnabled = Configuration.SsoEnabled;

                logger.LogMessage("Configuration loaded");
            }
            catch (Exception ex)
            {
                logger.LogError("Load Yaml failed", ex);
                return false;
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
