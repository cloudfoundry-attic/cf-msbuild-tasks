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
        public string AppGuid { get; set; }

        [Required]
        public string[] ServicesGuids { get; set; }

        [Output]
        public string[] BindingGuids { get; set; }

        public override bool Execute()
        {
            logger = new Microsoft.Build.Utilities.TaskLoggingHelper(this);

            CloudFoundryClient client = InitClient();

            logger.LogMessage("Binding services to app {0}", AppGuid);

            List<string> bindingGuids = new List<string>();

            foreach (string serviceGuid in ServicesGuids)
            {
                CreateServiceBindingRequest request = new CreateServiceBindingRequest();
                request.AppGuid = new Guid(AppGuid);
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

            BindingGuids = bindingGuids.ToArray();

            return true;
        }
    }
}
