namespace CloudFoundry.Build.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Build.Framework;

    public class SaveYaml : ITask
    {
        private TaskLogger logger;

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
      
        public bool Execute()
        {
            this.logger = new TaskLogger(this);
            this.logger.LogMessage("Saving configuration to {0}", this.CFConfigurationFile);

            try
            {
                PushProperties configuration = new PushProperties();
                configuration.AppDir = this.CFAppPath;
                configuration.Applications = new Dictionary<string, AppDetails>();
                configuration.Applications.Add(this.CFAppPath, new AppDetails() { Name = this.CFAppName, Url = this.CFRoute });
                configuration.AutoscaleInfo = new Autoscale() { Cpu = new Cpu() { MaxCpu = this.CFMaxCpu, MinCpu = this.CFMinCpu }, Enabled = this.CFEnabled != null ? this.CFEnabled : "no", InstancesInfo = new Instances() { MaxInstances = this.CFMaxInstances != 0 ? this.CFMaxInstances : 1, MinInstances = this.CFMinInstances != 0 ? this.CFMinInstances : 1 } };
                configuration.Disk = this.CFDisk != 0 ? this.CFDisk : 1024;
                configuration.Instances = this.CFInstancesNumber;
                configuration.Memory = this.CFAppMemory;
                configuration.Name = this.CFAppName;
                configuration.PlacementZone = this.CFPlacementZone != null ? this.CFPlacementZone : "default";
                if (this.CFServiceName != null)
                {
                    configuration.Services = new Dictionary<string, ServiceDetails>();
                    configuration.Services.Add(this.CFServiceName, new ServiceDetails() { Plan = this.CFServicePlan, Type = this.CFServiceType });
                }

                configuration.SsoEnabled = this.CFSsoEnabled != null ? this.CFSsoEnabled : "no";
                configuration.Stack = this.CFStack;

                Utils.SerializeToFile(configuration, this.CFConfigurationFile);
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
