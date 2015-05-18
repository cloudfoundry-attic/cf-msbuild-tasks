using Microsoft.Build.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudFoundry.Build.Tasks
{
    public class SaveYaml : ITask
    { 
        public IBuildEngine BuildEngine
        {
            get;
            set;
        }
        public ITaskHost HostObject
        {
            get;
            set;
        }

        [Required]
        public string CFAppPath { get; set; }

        [Required]
        public string CFAppName { get; set; }

        [Required]
        public string CFRoute { get; set; }

        public int CFMaxCpu { get; set; }

        public int CFMinCpu { get; set; }

        public string CFEnabled { get; set; }

        public int CFMaxInstances { get; set; }

        public int CFMinInstances { get; set; }

        public int CFDisk { get; set; }

        [Required]
        public int CFInstancesNumber { get; set; }
        
        [Required]
        public int CFAppMemory { get; set; }

        public string CFPlacementZone { get; set; }

        public string CFServiceName { get; set; }

        public string CFServicePlan { get; set; }

        public string CFServiceType { get; set; }

        public string CFSsoEnabled { get; set; }

        [Required]
        public string CFStack { get; set; }

        [Required]
        public string CFConfigurationFile { get; set; }

        private TaskLogger logger;
      
        public bool Execute()
        {
            logger = new TaskLogger(this);
            logger.LogMessage("Saving configuration to {0}", CFConfigurationFile);

            try
            {
                PushProperties Configuration = new PushProperties();
                Configuration.AppDir = CFAppPath;
                Configuration.Applications = new Dictionary<string, AppDetails>();
                Configuration.Applications.Add(CFAppPath, new AppDetails() { Name = CFAppName, Url = CFRoute });
                Configuration.AutoscaleInfo = new Autoscale() { Cpu = new Cpu() { MaxCpu = CFMaxCpu, MinCpu = CFMinCpu }, Enabled = CFEnabled != null ? CFEnabled : "no", InstancesInfo = new Instances() { MaxInstances = CFMaxInstances != 0 ? CFMaxInstances : 1, MinInstances = CFMinInstances != 0 ? CFMinInstances : 1 } };
                Configuration.Disk = CFDisk != 0 ? CFDisk : 1024;
                Configuration.Instances = CFInstancesNumber;
                Configuration.Memory = CFAppMemory;
                Configuration.Name = CFAppName;
                Configuration.PlacementZone = CFPlacementZone != null ? CFPlacementZone : "default";
                if (CFServiceName != null)
                {
                    Configuration.Services = new Dictionary<string, ServiceDetails>();
                    Configuration.Services.Add(CFServiceName, new ServiceDetails() { Plan = CFServicePlan, Type = CFServiceType });
                }

                Configuration.SsoEnabled = CFSsoEnabled != null ? CFSsoEnabled : "no";
                Configuration.Stack = CFStack;

                Utils.SerializeToFile(Configuration, CFConfigurationFile);
            }

            catch (Exception exception)
            {
                this.logger.LogError("Save Yaml failed", exception);
                return false;
            }
            return true;
        }

      
    }
}
