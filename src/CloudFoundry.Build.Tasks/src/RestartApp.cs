namespace CloudFoundry.Build.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using CloudFoundry.CloudController.V2.Client;
    using CloudFoundry.CloudController.V2.Client.Data;
    using CloudFoundry.Logyard.Client;
    using Microsoft.Build.Framework;

    public class RestartApp : BaseTask
    {
        [Required]
        public string CFAppGuid { get; set; }

        public override bool Execute()
        {
            this.Logger = new TaskLogger(this);
            Logger.LogMessage("Restarting application {0}", this.CFAppGuid);

            if (string.IsNullOrWhiteSpace(this.CFAppGuid))
            {
                Logger.LogError("Application Guid must be specified");
                return false;
            }

            try
            {
                CloudFoundryClient client = InitClient();

                // ======= HOOKUP LOGGING =======
                // TODO: detect logyard vs loggregator
                GetV1InfoResponse info = client.Info.GetV1Info().Result;

                using (LogyardLog logyard = new LogyardLog(new Uri(info.AppLogEndpoint), string.Format(CultureInfo.InvariantCulture, "bearer {0}", client.AuthorizationToken), null, CFSkipSslValidation))
                {
                    logyard.ErrorReceived += (sender, error) =>
                    {
                        Logger.LogErrorFromException(error.Error);
                    };

                    logyard.StreamOpened += (sender, args) =>
                    {
                        Logger.LogMessage("Log stream opened.");
                    };

                    logyard.StreamClosed += (sender, args) =>
                    {
                        Logger.LogMessage("Log stream closed.");
                    };

                    logyard.MessageReceived += (sender, message) =>
                    {
                        Logger.LogMessage("[{0}] - {1}: {2}", message.Message.Value.Source, message.Message.Value.HumanTime, message.Message.Value.Text);
                    };

                    logyard.StartLogStream(this.CFAppGuid, 0, true);

                    GetAppSummaryResponse response = client.Apps.GetAppSummary(new Guid(this.CFAppGuid)).Result;

                    if (response.State != "STOPPED")
                    {
                        UpdateAppRequest stopReq = new UpdateAppRequest();
                        stopReq.State = "STOPPED";
                        client.Apps.UpdateApp(new Guid(this.CFAppGuid), stopReq).Wait();
                    }

                    UpdateAppRequest startReq = new UpdateAppRequest();
                    startReq.State = "STARTED";
                    client.Apps.UpdateApp(new Guid(this.CFAppGuid), startReq).Wait();

                    // ======= WAIT FOR APP TO COME ONLINE =======
                    while (true)
                    {
                        GetAppSummaryResponse appSummary = client.Apps.GetAppSummary(new Guid(this.CFAppGuid)).Result;

                        if (appSummary.RunningInstances > 0)
                        {
                            break;
                        }

                        if (appSummary.PackageState == "FAILED")
                        {
                            Logger.LogError("App staging failed.");
                            return false;
                        }
                        else if (appSummary.PackageState == "PENDING")
                        {
                            Logger.LogMessage("App is staging ...");
                        }
                        else if (appSummary.PackageState == "STAGED")
                        {
                            Logger.LogMessage("App staged, waiting for it to come online ...");
                        }

                        Thread.Sleep(3000);
                    }
            
                    logyard.StopLogStream();
                }
            }
            catch (Exception exception)
            {
                this.Logger.LogError("Restart App failed", exception);
                return false;
            }

            return true;
        }
    }
}
