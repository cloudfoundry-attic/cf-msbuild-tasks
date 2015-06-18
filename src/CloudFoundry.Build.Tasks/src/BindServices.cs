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
        public string CFOrganization { get; set; }

        [Required]
        public string CFSpace { get; set; }

        [Output]
        public string[] CFBindingGuids { get; set; }

        public override bool Execute()
        {
            this.CFOrganization = this.CFOrganization.Trim();
            this.CFSpace = this.CFSpace.Trim();

            this.Logger = new TaskLogger(this);

            try
            {
                CloudFoundryClient client = InitClient();

                var app = LoadAppFromManifest();

                 Guid? spaceGuid = null;

                if ((!string.IsNullOrWhiteSpace(this.CFSpace)) && (!string.IsNullOrWhiteSpace(this.CFOrganization)))
                {
                    spaceGuid = Utils.GetSpaceGuid(client, this.Logger, this.CFOrganization, this.CFSpace);
                    if (spaceGuid == null)
                    {
                        return false;
                    }
                }

                var appGuid = Utils.GetAppGuid(client, app.Name, spaceGuid.Value);
                if (appGuid.HasValue == false)
                {
                    Logger.LogError("Could not find app {0} in space {1}", app.Name, this.CFSpace);
                    return false;
                }

                Logger.LogMessage("Binding services to app {0}", app.Name);

                List<string> bindingGuids = new List<string>();
                if (app.Services != null)
                {
                    foreach (string serviceName in app.Services)
                    {
                        var serviceGuid = Utils.GetServiceGuid(client, serviceName, spaceGuid.Value);

                        if (serviceGuid.HasValue)
                        {
                            CreateServiceBindingRequest request = new CreateServiceBindingRequest();
                            request.AppGuid = appGuid.Value;
                            request.ServiceInstanceGuid = serviceGuid.Value;

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
                        else
                        {
                            Logger.LogError("Could not find service instance {0}", serviceName);
                        }
                    }

                    this.CFBindingGuids = bindingGuids.ToArray();
                }
                else
                {
                    return true;
                }
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
