using CloudFoundry.CloudController.V2.Client;
using CloudFoundry.CloudController.V2.Client.Data;
using CloudFoundry.Logyard.Client;
using Microsoft.Build.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CloudFoundry.Build.Tasks
{
    public class RestartApp : BaseTask
    {
        [Required]
        public String CFAppGuid { get; set; }
        public override bool Execute()
        {

            logger = new Microsoft.Build.Utilities.TaskLoggingHelper(this);
            logger.LogMessage("Restarting application {0}", CFAppGuid);

            if (CFAppGuid.Length == 0)
            {
                logger.LogError("Application Guid must be specified");
                return false;
            }

            CloudFoundryClient client = InitClient();


            // ======= HOOKUP LOGGING =======
            GetV1InfoResponse info = client.Info.GetV1Info().Result;

            if (string.IsNullOrWhiteSpace(info.AppLogEndpoint) == false)
            {
                this.GetLogsUsingLogyard(client, new Guid(CFAppGuid), info);
            }
            else
            {
                GetInfoResponse detailedInfo = client.Info.GetInfo().Result;

                if (string.IsNullOrWhiteSpace(detailedInfo.LoggingEndpoint) == false)
                {
                    this.GetLogsUsingLoggregator(client, new Guid(CFAppGuid), detailedInfo);
                }
                else
                {
                    this.logger.LogError("Could not retrieve application logs");
                }
            }
            
            return true;
        }

        private void GetLogsUsingLoggregator(CloudFoundryClient client, Guid? appGuid, GetInfoResponse detailedInfo)
        {
            using (Loggregator.Client.LoggregatorLog loggregator = new Loggregator.Client.LoggregatorLog(new Uri(detailedInfo.LoggingEndpoint), string.Format(CultureInfo.InvariantCulture, "bearer {0}", client.AuthorizationToken),null, CFSkipSslValidation))
            {
                loggregator.ErrorReceived += (sender, error) =>
                {
                    logger.LogErrorFromException(error.Error);
                    if (error.Error.InnerException != null)
                    {
                        logger.LogErrorFromException(error.Error.InnerException);
                    }
                };

                loggregator.StreamOpened += (sender, args) =>
                {
                    logger.LogMessage("Log stream opened.");
                };

                loggregator.StreamClosed += (sender, args) =>
                {
                    logger.LogMessage("Log stream closed.");
                };

                loggregator.MessageReceived += (sender, message) =>
                {
                    var timeStamp = message.LogMessage.Timestamp / 1000 / 1000;
                    var time = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(timeStamp);
                    logger.LogMessage("[{0}] - {1}: {2}", message.LogMessage.SourceName, time.ToString(), message.LogMessage.Message);
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
                    logger.LogErrorFromException(error.Error);
                };

                logyard.StreamOpened += (sender, args) =>
                {
                    logger.LogMessage("Log stream opened.");
                };

                logyard.StreamClosed += (sender, args) =>
                {
                    logger.LogMessage("Log stream closed.");
                };

                logyard.MessageReceived += (sender, message) =>
                {
                    logger.LogMessage("[{0}] - {1}: {2}", message.Message.Value.Source, message.Message.Value.HumanTime, message.Message.Value.Text);
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
                    logger.LogMessage("App is staging ...");
                }
                else if (appSummary.PackageState == "STAGED")
                {
                    logger.LogMessage("App staged, waiting for it to come online ...");
                }

                Thread.Sleep(3000);
            }
        }
    }
}
