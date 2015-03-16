using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudFoundry.CloudController.V2.Client;
using CloudFoundry.CloudController.V2;
using System.Threading;
using CloudFoundry.UAA;
using System.IO;
using YamlDotNet;
using YamlDotNet.Serialization;
using CloudFoundry.CloudController.V2.Client.Data;

namespace CloudFoundry.Build
{
    public class PushJob
    {
        private CloudCredentials creds;
        private string configurationFile;
        private PushProperties configurationParameters;
        private CancellationToken cancellationToken;
        private Uri cloudTarget;
        private Guid spaceGuid;
        private string spaceName;
        public event EventHandler<PushProgressEventArgs> progressEvent;
       

        private const int StepCount = 6;
        private int usedStep = 0;

        public PushJob(CloudCredentials credentials, CancellationToken ct, Guid space, Uri serverUri, string ConfigurationFile)
        {
            this.creds = credentials;
            this.configurationFile = ConfigurationFile;
            this.cancellationToken = ct;
            this.cloudTarget = serverUri;
            this.spaceGuid = space;
        }

        public PushJob(CloudCredentials credentials, CancellationToken ct, string space, Uri serverUri, string ConfigurationFile)
        {
            this.creds = credentials;
            this.configurationFile = ConfigurationFile;
            this.cancellationToken = ct;
            this.cloudTarget = serverUri;
            this.spaceName = space;
        }

        public PushJob(CloudCredentials credentials, CancellationToken ct, Guid space, Uri serverUri, PushProperties Configuration)
        {
            this.creds = credentials;
            this.configurationParameters = Configuration;
            this.cancellationToken = ct;
            this.cloudTarget = serverUri;
            this.spaceGuid = space;
        }

        public PushJob(CloudCredentials credentials, CancellationToken ct, string spaceName, Uri serverUri, PushProperties Configuration)
        {
            this.creds = credentials;
            this.configurationParameters = Configuration;
            this.cancellationToken = ct;
            this.cloudTarget = serverUri;
            this.spaceName = spaceName;
        }


        public async Task Start()
        {
            if (!File.Exists(configurationFile))
            {
                if (!ValidateParameters(configurationParameters))
                {
                    throw new Exception("Configuration parameters are not valid");
                }
                else
                {
                    if (configurationFile.Length > 0)
                    {
                        SerializeToFile(configurationParameters, configurationFile);
                    }
                }
            }
            else
            {
                configurationParameters =  DeserializeFromFile(configurationFile);
                
                if (!ValidateParameters(configurationParameters))
                {
                    throw new Exception("Configuration parameters are not valid");
                }
               
            }

            CloudFoundryClient client = new CloudFoundryClient(this.cloudTarget, this.cancellationToken);
            await client.Login(this.creds);
            
            //skip ssl
            System.Net.ServicePointManager.ServerCertificateValidationCallback = ((sender, certificate, chain, sslPolicyErrors) => true);

            if (spaceName != string.Empty)
            {
                PagedResponseCollection<ListAllSpacesResponse> spaceList = await client.Spaces.ListAllSpaces();

                var space = spaceList.Where(o => o.Name == spaceName).FirstOrDefault();

                if (space == null)
                {
                    throw new Exception(string.Format("Space {0} not found", spaceName));
                }
                spaceGuid = new Guid(space.EntityMetadata.Guid);
            }

            usedStep += 1;
            this.TriggerProgressEvent(usedStep, "Checking if application exists");
            //Step 1 - Check if application exists
            Guid? appGuid = await CheckAppExists(client);

            usedStep += 1;
            //Step 2 - Create/update application
            if (appGuid.HasValue)
            {
                this.TriggerProgressEvent(usedStep, "Updating application");
                UpdateAppRequest updateApp = new UpdateAppRequest();
                updateApp.Name = configurationParameters.name;
                updateApp.Instances = configurationParameters.autoscale.instances.min;
                updateApp.Memory = configurationParameters.memory;

                await client.Apps.UpdateApp(appGuid, updateApp);
            }
            else
            {
                Guid? stackGuid = await GetStackGuid(client, configurationParameters.stack);

                if (stackGuid.HasValue)
                {
                    this.TriggerProgressEvent(usedStep, "Creating application");
                    CreateAppRequest createApp = new CreateAppRequest();
                    createApp.Name = configurationParameters.name;
                    createApp.SpaceGuid = spaceGuid;
                    createApp.StackGuid = stackGuid.Value;

                    CreateAppResponse appResponse = await client.Apps.CreateApp(createApp);

                    appGuid = new Guid(appResponse.EntityMetadata.Guid);
                }
                else
                {
                    throw new Exception("Specified stack not found on server");
                }
            }

            usedStep += 1;
            this.TriggerProgressEvent(usedStep, "Pushing application");
            //Step 3 - Push application without start
            client.Apps.PushProgress += Apps_PushProgress;
            await client.Apps.Push(appGuid.Value, configurationParameters.app_dir, false);

            usedStep += 1;
            this.TriggerProgressEvent(usedStep, "Mapping urls");
            //Step 4 - Map urls
            await MapUrls(client, appGuid.Value, spaceGuid, configurationParameters.applications.Values);

            usedStep += 1;
            this.TriggerProgressEvent(usedStep, "Creating and binding service instances");
            //Step 5 - Create and bind services
            await CreateServices(client, appGuid, spaceGuid, configurationParameters.services);

            usedStep += 1;
            this.TriggerProgressEvent(usedStep, "Starting application");
            //Step 6 - Start application
            await StartApplication(client, appGuid);

            this.TriggerProgressEvent(usedStep, "Push job finished");
        }

        void Apps_PushProgress(object sender, PushProgressEventArgs e)
        {
            this.TriggerProgressEvent(0, string.Format("{0} - {1}%", e.Message, e.Percent));
        }

        private async Task StartApplication(CloudFoundryClient client, Guid? appGuid)
        {
            UpdateAppRequest update = new UpdateAppRequest();
            update.State = "STARTED";
            await client.Apps.UpdateApp(appGuid, update);
        }

        private async Task CreateServices(CloudFoundryClient client, Guid? appGuid, Guid spaceGuid, Dictionary<string, ServiceDetails> servicesInfo)
        {
            PagedResponseCollection<ListAllServicePlansResponse> servicePlanList = await client.ServicePlans.ListAllServicePlans();

            foreach (var serviceInfo in servicesInfo)
            {
                PagedResponseCollection<ListAllServiceInstancesForSpaceResponse> existingServices = await client.Spaces.ListAllServiceInstancesForSpace(spaceGuid);

                var existingService = existingServices.Where(service => service.Name == serviceInfo.Key).FirstOrDefault();

                if (existingService == null)
                {
                    Guid? neededPlanGuid = null;
                    foreach (ListAllServicePlansResponse servicePlan in servicePlanList)
                    {
                        if (serviceInfo.Value.plan == servicePlan.Name)
                        {
                            RetrieveServiceResponse service = await client.Services.RetrieveService(servicePlan.ServiceGuid);
                            if (service.Label == serviceInfo.Value.type)
                            {
                                neededPlanGuid = new Guid(servicePlan.EntityMetadata.Guid);
                                break;
                            }
                        }
                    }

                    if (neededPlanGuid.HasValue)
                    {
                        CreateServiceInstanceRequest createService = new CreateServiceInstanceRequest();
                        createService.Name = serviceInfo.Key;
                        createService.ServicePlanGuid = neededPlanGuid;
                        createService.SpaceGuid = spaceGuid;
                        CreateServiceInstanceResponse responseInstance =  await client.ServiceInstances.CreateServiceInstance(createService);

                        await BindService(client, appGuid, new Guid(responseInstance.EntityMetadata.Guid));
                    }
                    else
                    {
                        throw new Exception("Requested plan does not exist for service type, service not created !");
                    }
                }
                else
                {
                    await BindService(client, appGuid, new Guid(existingService.EntityMetadata.Guid));
                }
            }
        }

        private static async Task BindService(CloudFoundryClient client, Guid? appGuid, Guid? serviceInstanceGuid)
        {
            CreateServiceBindingRequest bind = new CreateServiceBindingRequest();
            bind.AppGuid = appGuid;
            bind.ServiceInstanceGuid = serviceInstanceGuid;

            await client.ServiceBindings.CreateServiceBinding(bind);
        }

        private async Task MapUrls(CloudFoundryClient client, Guid appGuid, Guid spaceGuid, Dictionary<string, AppDetails>.ValueCollection valueCollection)
        {
            var appSummary = await client.Apps.GetAppSummary(appGuid);

            foreach (AppDetails detail in valueCollection)
            {
                bool exists = false;

                foreach (var r in appSummary.Routes)
                {
                    var routeUri = string.Format("{0}.{1}", r["host"].ToString(), r["domain"]["name"].ToString());
                    if (routeUri.ToLower() == detail.url.ToLower())
                    {
                        exists = true;
                        break;
                    }
                }
                if (exists)
                {
                    continue;
                }

                string domain = detail.url.Substring(detail.url.IndexOf('.') + 1);
                PagedResponseCollection<ListAllDomainsDeprecatedResponse> domainInfoList = await client.DomainsDeprecated.ListAllDomainsDeprecated();

                ListAllDomainsDeprecatedResponse domainInfo = domainInfoList.Where(o => o.Name == domain).FirstOrDefault();
                CreateRouteRequest req = new CreateRouteRequest();
                req.DomainGuid = new Guid(domainInfo.EntityMetadata.Guid);
                req.SpaceGuid = spaceGuid;
                req.Host = detail.url.Split('.').First().ToLower();

                CreateRouteResponse response = await client.Routes.CreateRoute(req);


                AssociateRouteWithAppResponse asocReply = await client.Apps.AssociateRouteWithApp(appGuid, new Guid(response.EntityMetadata.Guid));
            }
        }

        private async Task<Guid?> GetStackGuid(CloudFoundryClient client, string stackName)
        {
            PagedResponseCollection<ListAllStacksResponse> stackList = await client.Stacks.ListAllStacks();
            foreach (var stack in stackList)
            {
                if (stack.Name == stackName)
                {
                    return new Guid(stack.EntityMetadata.Guid);
                }
            }
            return null;
        }

        private async Task<Guid?> CheckAppExists(CloudFoundryClient client)
        {
            PagedResponseCollection<ListAllAppsForSpaceResponse> appList = await client.Spaces.ListAllAppsForSpace(this.spaceGuid);
            foreach (var app in appList)
            {
                if (app.Name == this.configurationParameters.name)
                {
                    return new Guid(app.EntityMetadata.Guid);
                }
            }
            return null;
        }

        private void TriggerProgressEvent(int currentStep, string message)
        {
            if (this.progressEvent != null)
            {
                this.progressEvent(this,new PushProgressEventArgs(){ Message=message, Percent=(int)((double)currentStep/(double)(StepCount)*100)});
            }
        }
        private bool ValidateParameters(PushProperties ConfigurationParameters)
        {
            bool ok = true;
            if (!Directory.Exists(ConfigurationParameters.app_dir))
            {
                ok = false;
                throw new Exception("Directory does not exist");
            }
            if (ConfigurationParameters.name == string.Empty)
            {
                ok = false;
                throw new Exception("Application name must not be empty");
            }
            if (ConfigurationParameters.stack == string.Empty)
            {
                ok = false;
                throw new Exception("Stack must not be empty");
            }
            if (ConfigurationParameters.memory == 0)
            {
                ok = false;
                throw new Exception("Memory must be specified as an integer value");
            }
            return ok;
        }


        private static PushProperties DeserializeFromFile(String FilePath)
        {
            PushProperties returnValue;
            Deserializer deserializer = new Deserializer();
            string content = File.ReadAllText(FilePath).Replace("-", "_");
            using (TextReader reader = new StringReader(content))
            {
                returnValue = deserializer.Deserialize(reader, typeof(PushProperties)) as PushProperties;
            }

            return returnValue;
        }

        private static void SerializeToFile(PushProperties ConfigurationParameters, String FilePath)
        {
            Serializer serializer = new Serializer(YamlDotNet.Serialization.SerializationOptions.EmitDefaults, null);

            StringBuilder builder = new StringBuilder();

            using (TextWriter writer = new StringWriter(builder))
            {
                serializer.Serialize(writer, ConfigurationParameters);
            }

            File.WriteAllText(FilePath, builder.ToString().Replace("_", "-"));
        }
    }
}
