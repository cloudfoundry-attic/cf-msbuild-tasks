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
        public string CFOrganization { get; set; }

        [Required]
        public string CFSpace { get; set; }

        public override bool Execute()
        {
            this.CFOrganization = this.CFOrganization.Trim();
            this.CFSpace = this.CFSpace.Trim();

            this.Logger = new TaskLogger(this);
            var app = LoadAppFromManifest();

            try
            {
                CloudFoundryClient client = InitClient();

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
                    Logger.LogError("Application {0} not found", app.Name);
                    return false;
                }

                Logger.LogMessage("Restarting application {0}", app.Name);

                // ======= HOOKUP LOGGING =======
                GetV1InfoResponse info = client.Info.GetV1Info().Result;

                if (string.IsNullOrWhiteSpace(info.AppLogEndpoint) == false)
                {
                    this.GetLogsUsingLogyard(client, appGuid, info);
                }
                else
                {
                    GetInfoResponse detailedInfo = client.Info.GetInfo().Result;

                    if (string.IsNullOrWhiteSpace(detailedInfo.LoggingEndpoint) == false)
                    {
                        this.GetLogsUsingLoggregator(client, appGuid, detailedInfo);
                    }
                    else
                    {
                        this.Logger.LogError("Could not retrieve application logs");
                    }
                }
            }
            catch (Exception exception)
            {
                this.Logger.LogError("Restart App failed", exception);
                return false;
            }

            return true;
        }

        private void GetLogsUsingLoggregator(CloudFoundryClient client, Guid? appGuid, GetInfoResponse detailedInfo)
        {
            using (Loggregator.Client.LoggregatorLog loggregator = new Loggregator.Client.LoggregatorLog(new Uri(detailedInfo.LoggingEndpoint), string.Format(CultureInfo.InvariantCulture, "bearer {0}", client.AuthorizationToken), null, this.CFSkipSslValidation))
            {
                loggregator.ErrorReceived += (sender, error) =>
                {
                    Logger.LogErrorFromException(error.Error);
                };

                loggregator.StreamOpened += (sender, args) =>
                {
                    Logger.LogMessage("Log stream opened.");
                };

                loggregator.StreamClosed += (sender, args) =>
                {
                    Logger.LogMessage("Log stream closed.");
                };

                loggregator.MessageReceived += (sender, message) =>
                {
                    long timeInMilliSeconds = message.LogMessage.Timestamp / 1000 / 1000;
                    var logTimeStamp = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(timeInMilliSeconds);

                    Logger.LogMessage("[{0}] - {1}: {2}", message.LogMessage.SourceName, logTimeStamp.ToString(), message.LogMessage.Message);
                };

                loggregator.Tail(appGuid.Value.ToString());

                this.MonitorApp(client, appGuid);

                loggregator.StopLogStream();
            }
        }

        private void GetLogsUsingLogyard(CloudFoundryClient client, Guid? appGuid, GetV1InfoResponse info)
        {
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

                logyard.StartLogStream(appGuid.Value.ToString(), 0, true);

                this.MonitorApp(client, appGuid);

                logyard.StopLogStream();
            }
        }

        private void MonitorApp(CloudFoundryClient client, Guid? appGuid)
        {
            GetAppSummaryResponse response = client.Apps.GetAppSummary(appGuid.Value).Result;

            if (response.State != "STOPPED")
            {
                UpdateAppRequest stopReq = new UpdateAppRequest();
                stopReq.State = "STOPPED";
                client.Apps.UpdateApp(appGuid.Value, stopReq).Wait();
            }

            UpdateAppRequest startReq = new UpdateAppRequest();
            startReq.State = "STARTED";
            client.Apps.UpdateApp(appGuid.Value, startReq).Wait();

            // ======= WAIT FOR APP TO COME ONLINE =======
            while (true)
            {
                if (this.CancelToken.IsCancellationRequested == true)
                {
                    break;
                }

                GetAppSummaryResponse appSummary = client.Apps.GetAppSummary(appGuid.Value).Result;

                if (appSummary.RunningInstances > 0)
                {
                    break;
                }

                if (appSummary.PackageState == "FAILED")
                {
                    throw new InvalidOperationException("App staging failed");
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
        }
    }
}
