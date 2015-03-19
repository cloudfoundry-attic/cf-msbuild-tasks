using CloudFoundry.CloudController.V2;
using CloudFoundry.UAA;
using Microsoft.Build.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudFoundry.Build.Tasks
{
    public class BindRoutes : BaseTask
    {
        [Required]
        public String AppGuid { get; set; }

        [Required]
        public String[] RouteGuids { get; set; }
        public override bool Execute()
        {
           
            logger = new Microsoft.Build.Utilities.TaskLoggingHelper(this);

            if (AppGuid.Length == 0)
            {
                logger.LogError("Application Guid must be specified");
                return false;
            }

            CloudFoundryClient client = InitClient();
            
            logger.LogMessage("Binding routes to application {0}", AppGuid);
            foreach (string routeGuid in RouteGuids)
            {
                client.Apps.AssociateRouteWithApp(new Guid(AppGuid), new Guid(routeGuid)).Wait();
            }
            
            return true;
        }
    }
}
