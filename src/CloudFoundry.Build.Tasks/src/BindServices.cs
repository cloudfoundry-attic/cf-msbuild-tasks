namespace CloudFoundry.Build.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using CloudFoundry.CloudController.Common.Exceptions;
    using CloudFoundry.CloudController.V2.Client;
    using CloudFoundry.CloudController.V2.Client.Data;
    using Microsoft.Build.Framework;

    public class BindServices : BaseTask
    {
        [Required]
        public string CFAppGuid { get; set; }

        [Required]
        public string[] CFServicesGuids { get; set; }

        [Output]
        public string[] CFBindingGuids { get; set; }

        public override bool Execute()
        {
            this.Logger = new TaskLogger(this);

            try
            {
                CloudFoundryClient client = InitClient();

                Logger.LogMessage("Binding services to app {0}", this.CFAppGuid);

                List<string> bindingGuids = new List<string>();

                foreach (string serviceGuid in this.CFServicesGuids)
                {
                    CreateServiceBindingRequest request = new CreateServiceBindingRequest();
                    request.AppGuid = new Guid(this.CFAppGuid);
                    request.ServiceInstanceGuid = new Guid(serviceGuid);

                    try
                    {
                        var result = client.ServiceBindings.CreateServiceBinding(request).Result;
                        bindingGuids.Add(result.EntityMetadata.Guid);
                    }
                    catch (AggregateException ex)
                    {
                        foreach (Exception e in ex.Flatten().InnerExceptions)
                        {
                            if (e is CloudFoundryException)
                            {
                                Logger.LogWarning(e.Message);
                            }
                            else
                            {
                                this.Logger.LogError("Bind Services failed", ex);
                                return false;
                            }
                        }
                    }
                }

                this.CFBindingGuids = bindingGuids.ToArray();
            }
            catch (Exception exception)
            {
                this.Logger.LogError("Bind Services failed", exception);
                return false;
            }

            return true;
        }
    }
}
