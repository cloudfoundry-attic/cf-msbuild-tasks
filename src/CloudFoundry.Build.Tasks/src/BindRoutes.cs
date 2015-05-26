using CloudFoundry.CloudController.V2.Client;
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
        public String CFAppGuid { get; set; }

        [Required]
        public String[] CFRouteGuids { get; set; }
        public override bool Execute()
        {
            logger = new TaskLogger(this);

            if (string.IsNullOrWhiteSpace(CFAppGuid))
            {
                logger.LogError("Application Guid must be specified");
                return false;
            }

            try
            {
                CloudFoundryClient client = InitClient();

                logger.LogMessage("Binding routes to application {0}", CFAppGuid);
                foreach (string routeGuid in CFRouteGuids)
                {
                    client.Apps.AssociateRouteWithApp(new Guid(CFAppGuid), new Guid(routeGuid)).Wait();
                }
            }
            catch (Exception exception)
            {
                this.logger.LogError("Bind Routes failed", exception);
                return false;
            }
            return true;
        }
    }
}
