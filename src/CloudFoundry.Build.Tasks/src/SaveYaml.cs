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
        public string AppPath { get; set; }

        [Required]
        public string AppName { get; set; }

        [Required]
        public string Route { get; set; }

        public int MaxCpu { get; set; }

        public int MinCpu { get; set; }

        public string Enabled { get; set; }

        public int MaxInstances { get; set; }

        public int MinInstances { get; set; }

        public int Disk { get; set; }

        [Required]
        public int InstancesNumber { get; set; }
        
        [Required]
        public int Memory { get; set; }

        public string PlacementZone { get; set; }

        public string ServiceName { get; set; }

        public string ServicePlan { get; set; }

        public string ServiceType { get; set; }

        public string SsoEnabled { get; set; }

        [Required]
        public string Stack { get; set; }

        [Required]
        public string ConfigurationFile { get; set; }

        private Microsoft.Build.Utilities.TaskLoggingHelper logger;
      
        public bool Execute()
        {
            logger = new Microsoft.Build.Utilities.TaskLoggingHelper(this);
            logger.LogMessage("Saving configuration to {0}", ConfigurationFile);
            
            PushProperties Configuration = new PushProperties();
            Configuration.AppDir = AppPath;
            Configuration.Applications = new Dictionary<string, AppDetails>();
            Configuration.Applications.Add(AppPath, new AppDetails() { Name = AppName, Url = Route });
            Configuration.AutoscaleInfo = new Autoscale() { Cpu = new Cpu() { MaxCpu = MaxCpu, MinCpu = MinCpu }, Enabled = Enabled !=null ? Enabled: "no", InstancesInfo = new Instances() { MaxInstances = MaxInstances !=0 ? MaxInstances : 1, MinInstances = MinInstances != 0 ? MinInstances : 1 } };
            Configuration.Disk = Disk!=0 ? Disk : 1024;
            Configuration.Instances = InstancesNumber;
            Configuration.Memory = Memory;
            Configuration.Name = AppName;
            Configuration.PlacementZone = PlacementZone !=null ? PlacementZone : "default";
            if (ServiceName != null)
            {
                Configuration.Services = new Dictionary<string, ServiceDetails>();
                Configuration.Services.Add(ServiceName, new ServiceDetails() { Plan = ServicePlan, Type = ServiceType });
            }
            
            Configuration.SsoEnabled = SsoEnabled!=null ? SsoEnabled : "no";
            Configuration.Stack = Stack;

            Utils.SerializeToFile(Configuration, ConfigurationFile);

            return true;
        }

      
    }
}
