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
            logger = new Microsoft.Build.Utilities.TaskLoggingHelper(this);

            try 
            { 
                if (CFAppGuid.Length == 0)
                {
                    logger.LogError("Application Guid must be specified");
                    return false;
                }

                CloudFoundryClient client = InitClient();

                logger.LogMessage("Binding routes to application {0}", CFAppGuid);
                foreach (string routeGuid in CFRouteGuids)
                {
                    client.Apps.AssociateRouteWithApp(new Guid(CFAppGuid), new Guid(routeGuid)).Wait();
                }
            }
            catch (AggregateException exception)
            {
                List<string> messages = new List<string>();
                ErrorFormatter.FormatExceptionMessage(exception, messages);
                this.logger.LogError(string.Join(Environment.NewLine, messages));
                return false;
            }
            return true;
        }
    }
}
