namespace CloudFoundry.Build.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Build.Framework;

    public class LoadYaml : ITask
    {
        private IBuildEngine buildEngine;

        private ITaskHost taskHost;

        private TaskLogger logger;
        
        public IBuildEngine BuildEngine
        {
            get
            { 
                return this.buildEngine; 
            }

            set
            {
                this.buildEngine = value; 
            }
        }

        public ITaskHost HostObject
        {
            get
            { 
                return this.taskHost;
            }

            set
            { 
                this.taskHost = value; 
            }
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Log every exception")]
        public bool Execute()
        {
            this.logger = new TaskLogger(this);
            this.logger.LogMessage("Loading configuration from {0}", this.CFConfigurationFile);
            try
            {
                this.Configuration = Utils.DeserializeFromFile(this.CFConfigurationFile);
                this.CFStack = this.Configuration.Stack;
                this.CFAppName = this.Configuration.Name;
                this.CFAppPath = this.Configuration.AppDir;

                List<string> urlList = new List<string>();
                foreach (var app in this.Configuration.Applications)
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
                    this.logger.LogMessage("Loaded url {0}", s);
                }

                this.CFRoutes = string.Join(";", urlList);

                this.CFAppMemory = this.Configuration.Memory;
                this.CFAppInstances = this.Configuration.Instances;
                this.CFAutoscale = Utils.Serialize<Autoscale>(this.Configuration.AutoscaleInfo);
                this.CFDisk = this.Configuration.Disk;

                if (this.Configuration.Services != null)
                {
                    List<ProvisionedService> servicesList = new List<ProvisionedService>();
                    foreach (var service in this.Configuration.Services)
                    {
                        servicesList.Add(new ProvisionedService() { Name = service.Key, Plan = service.Value.Plan, Type = service.Value.Type });
                    }

                    this.CFServices = Utils.Serialize<List<ProvisionedService>>(servicesList);
                }

                this.CFPlacementZone = this.Configuration.PlacementZone;
                this.CFSsoEnabled = this.Configuration.SsoEnabled;

                this.logger.LogMessage("Configuration loaded");
            }
            catch (Exception ex)
            {
                this.logger.LogError("Load Yaml failed", ex);
                return false;
            }

            return true;
        }
    }
}
