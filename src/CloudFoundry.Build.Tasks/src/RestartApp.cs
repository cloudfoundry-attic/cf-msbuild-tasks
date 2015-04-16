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
            // TODO: detect logyard vs loggregator

            GetV1InfoResponse v1Info = client.Info.GetV1Info().Result;

            using (LogyardLog logyard = new LogyardLog(new Uri(v1Info.AppLogEndpoint), string.Format(CultureInfo.InvariantCulture, "bearer {0}", client.AuthorizationToken)))
            {

                logyard.ErrorReceived += (sender, error) =>
                {
                    logger.LogErrorFromException(error.Error, true);
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

                logyard.StartLogStream(CFAppGuid, 0, true);


                GetAppSummaryResponse response = client.Apps.GetAppSummary(new Guid(CFAppGuid)).Result;

                if (response.State != "STOPPED")
                {
                    UpdateAppRequest stopReq = new UpdateAppRequest();
                    stopReq.State = "STOPPED";
                    client.Apps.UpdateApp(new Guid(CFAppGuid), stopReq).Wait();
                }

                UpdateAppRequest startReq = new UpdateAppRequest();
                startReq.State = "STARTED";
                client.Apps.UpdateApp(new Guid(CFAppGuid), startReq).Wait();

                // ======= WAIT FOR APP TO COME ONLINE =======
                while (true)
                {
                    GetAppSummaryResponse appSummary = client.Apps.GetAppSummary(new Guid(CFAppGuid)).Result;

                    if (appSummary.RunningInstances > 0)
                    {
                        break;
                    }

                    if (appSummary.PackageState == "FAILED")
                    {
                        logger.LogError("App staging failed.");
                        return false;
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

                logyard.StopLogStream();

            }

            return true;
        }
    }
}
