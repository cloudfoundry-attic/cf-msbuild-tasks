namespace CloudFoundry.Build.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using CloudFoundry.CloudController.V2.Client;
    using CloudFoundry.UAA;
    using CloudFoundry.UAA.Exceptions;
    using Microsoft.Build.Framework;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Login", Justification = "Using login name preferred")]
    public class LoginTask : BaseTask
    {
        [Output]
        public new string CFRefreshToken { get; set; }

        public override bool Execute()
        {
            this.Logger = new TaskLogger(this);
            try
            {
                this.InitClient();
            }
            catch (Exception exception)
            {
                this.Logger.LogError("Login failed", exception);
                return false;
            }

            return true;
        }
    }
}
