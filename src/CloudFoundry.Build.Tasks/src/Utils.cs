using CloudFoundry.CloudController.V2.Client;
using CloudFoundry.CloudController.V2.Client.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using YamlDotNet.Serialization;

namespace CloudFoundry.Build.Tasks
{
    internal static class Utils
    {
        internal static PushProperties DeserializeFromFile(String FilePath)
        {
            PushProperties returnValue;
            Deserializer deserializer = new Deserializer();
            string content = File.ReadAllText(FilePath);
            using (TextReader reader = new StringReader(content))
            {
                returnValue = deserializer.Deserialize(reader, typeof(PushProperties)) as PushProperties;
            }

            return returnValue;
        }

        internal static void SerializeToFile(PushProperties ConfigurationParameters, String FilePath)
        {
            Serializer serializer = new Serializer(YamlDotNet.Serialization.SerializationOptions.EmitDefaults, null);

            StringBuilder builder = new StringBuilder();

            using (TextWriter writer = new StringWriter(builder,CultureInfo.InvariantCulture))
            {
                serializer.Serialize(writer, ConfigurationParameters);
            }

            File.WriteAllText(FilePath, builder.ToString());
        }

        internal static string Serialize<T>(T value)
        {

            if (value == null)
            {
                return null;
            }

            XmlSerializer serializer = new XmlSerializer(typeof(T));

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Encoding = new UnicodeEncoding(false, false); // no BOM in a .NET string
            settings.Indent = false;
            settings.OmitXmlDeclaration = false;

            using (StringWriter textWriter = new StringWriter(CultureInfo.InvariantCulture))
            {
                using (XmlWriter xmlWriter = XmlWriter.Create(textWriter, settings))
                {
                    serializer.Serialize(xmlWriter, value);
                }
                return textWriter.ToString();
            }
        }

        internal static T Deserialize<T>(string xml)
        {

            if (string.IsNullOrEmpty(xml))
            {
                return default(T);
            }

            XmlSerializer serializer = new XmlSerializer(typeof(T));

            XmlReaderSettings settings = new XmlReaderSettings();
            // No settings need modifying here

            using (StringReader textReader = new StringReader(xml))
            {
                using (XmlReader xmlReader = XmlReader.Create(textReader, settings))
                {
                    return (T)serializer.Deserialize(xmlReader);
                }
            }

        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "Normalization to lowercase is better for urls")]
        internal static void ExtractDomainAndHost(string Route, out string domain, out string host)
        {
            Route = Route.Replace("http://", string.Empty).Replace("https://", string.Empty);
            domain = Route.Substring(Route.IndexOf('.') + 1);
            host = Route.Split('.').First().ToLower(CultureInfo.InvariantCulture);
        }

        internal static bool CheckForExistingService(string ServiceName, Guid? planGuid, CloudFoundryClient client)
        {
            bool exists = false;

            PagedResponseCollection<ListAllServiceInstancesResponse> serviceInstances = client.ServiceInstances.ListAllServiceInstances().Result;

            foreach (ListAllServiceInstancesResponse serviceInstanceDetails in serviceInstances)
            {
                if (serviceInstanceDetails.ServicePlanGuid.HasValue)
                {
                    if (serviceInstanceDetails.Name == ServiceName && serviceInstanceDetails.ServicePlanGuid.Value == planGuid)
                    {
                        exists = true;
                        break;
                    }
                }
            }

            return exists;
        }
    }
}
