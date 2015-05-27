namespace CloudFoundry.Build.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using CloudFoundry.CloudController.V2.Client;
    using CloudFoundry.UAA;
    using Microsoft.Build.Framework;

    public class BindRoutes : BaseTask
    {
        [Required]
        public string CFAppGuid { get; set; }

        [Required]
        public string[] CFRouteGuids { get; set; }

        public override bool Execute()
        {
            this.Logger = new TaskLogger(this);

            if (string.IsNullOrWhiteSpace(this.CFAppGuid))
            {
                this.Logger.LogError("Application Guid must be specified");
                return false;
            }

            try
            {
                CloudFoundryClient client = InitClient();

                Logger.LogMessage("Binding routes to application {0}", this.CFAppGuid);
                foreach (string routeGuid in this.CFRouteGuids)
                {
                    client.Apps.AssociateRouteWithApp(new Guid(this.CFAppGuid), new Guid(routeGuid)).Wait();
                }
            }
            catch (Exception exception)
            {
                this.Logger.LogError("Bind Routes failed", exception);
                return false;
            }

            return true;
        }
    }
}
