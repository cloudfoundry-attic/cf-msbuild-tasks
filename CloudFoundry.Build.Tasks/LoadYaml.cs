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
        public PushProperties Configuration {get;set;} //TODO: Figure out how to output whole configuration
        public bool Execute()
        {
            logger = new Microsoft.Build.Utilities.TaskLoggingHelper(this);
            try
            {
                Configuration = Utils.DeserializeFromFile(ConfigurationFile);
                
            }
            catch (Exception ex)
            {
                logger.LogErrorFromException(ex);
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
