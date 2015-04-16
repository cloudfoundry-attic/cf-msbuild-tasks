using CloudFoundry.CloudController.Common.Exceptions;
using CloudFoundry.CloudController.V2.Client;
using CloudFoundry.CloudController.V2.Client.Data;
using Microsoft.Build.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudFoundry.Build.Tasks
{
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
            logger = new Microsoft.Build.Utilities.TaskLoggingHelper(this);

            CloudFoundryClient client = InitClient();

            logger.LogMessage("Binding services to app {0}", CFAppGuid);

            List<string> bindingGuids = new List<string>();

            foreach (string serviceGuid in CFServicesGuids)
            {
                CreateServiceBindingRequest request = new CreateServiceBindingRequest();
                request.AppGuid = new Guid(CFAppGuid);
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
                            logger.LogWarning(e.Message);
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
            }

            CFBindingGuids = bindingGuids.ToArray();

            return true;
        }
    }
}
