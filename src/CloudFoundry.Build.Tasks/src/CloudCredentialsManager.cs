using Simple.CredentialManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CloudFoundry.Build.Tasks
{
    internal static class CloudCredentialsManager
    {
        private static string GetTargetString(Uri targetUri, string username)
        {
            UriBuilder uriBuilder = new UriBuilder(targetUri);
            uriBuilder.UserName = HttpUtility.UrlEncode(username);
            return uriBuilder.Uri.AbsoluteUri;
        }

        public static string GetPassword(Uri targetUri, string username)
        {
            using (Credential creds = new Credential())
            {
                creds.Target = GetTargetString(targetUri, username);
                creds.PersistenceType = PersistenceType.LocalComputer;

                if (creds.Exists())
                {
                    creds.Load();
                    return creds.Password;
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
