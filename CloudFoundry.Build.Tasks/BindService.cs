using CloudFoundry.CloudController.Common.Exceptions;
using CloudFoundry.CloudController.V2;
using CloudFoundry.CloudController.V2.Client.Data;
using CloudFoundry.UAA;
using Microsoft.Build.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudFoundry.Build.Tasks
{
    public class BindService : BaseTask
    {
        [Required]
        public String AppGuid { get; set; }

        [Required]
        public String ServiceGuid { get; set; }

        [Output]
        public String BindingGuid { get; set; }

        public override bool Execute()
        {   
            logger = new Microsoft.Build.Utilities.TaskLoggingHelper(this);

            CloudFoundryClient client = InitClient();

            logger.LogMessage("Binding services to app {0}", AppGuid);
            CreateServiceBindingRequest request = new CreateServiceBindingRequest();
            request.AppGuid = new Guid(AppGuid);
            request.ServiceInstanceGuid = new Guid(ServiceGuid);

            try
            {
                var result = client.ServiceBindings.CreateServiceBinding(request).Result;
                BindingGuid = result.EntityMetadata.Guid;
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
                        throw ex;
                    }
                }
            }

            return true;
        }
    }
}
