namespace CloudFoundry.Build.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml;
    using System.Xml.Serialization;
    using CloudFoundry.CloudController.V2.Client;
    using CloudFoundry.CloudController.V2.Client.Data;
    using YamlDotNet.Serialization;

    internal static class Utils
    {
        internal static PushProperties DeserializeFromFile(string filePath)
        {
            PushProperties returnValue;
            Deserializer deserializer = new Deserializer();
            string content = File.ReadAllText(filePath);
            using (TextReader reader = new StringReader(content))
            {
                returnValue = deserializer.Deserialize(reader, typeof(PushProperties)) as PushProperties;
            }

            return returnValue;
        }

        internal static void SerializeToFile(PushProperties configurationParameters, string filePath)
        {
            Serializer serializer = new Serializer(YamlDotNet.Serialization.SerializationOptions.EmitDefaults, null);

            StringBuilder builder = new StringBuilder();

            using (TextWriter writer = new StringWriter(builder, CultureInfo.InvariantCulture))
            {
                serializer.Serialize(writer, configurationParameters);
            }

            File.WriteAllText(filePath, builder.ToString());
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
        internal static void ExtractDomainAndHost(string route, out string domain, out string host)
        {
            route = route.Replace("http://", string.Empty).Replace("https://", string.Empty);
            domain = route.Substring(route.IndexOf('.') + 1);
            host = route.Split('.').First().ToLower(CultureInfo.InvariantCulture);
        }

        internal static Guid? CheckForExistingService(string serviceName, Guid? planGuid, CloudFoundryClient client)
        {
            PagedResponseCollection<ListAllServiceInstancesResponse> serviceInstances = client.ServiceInstances.ListAllServiceInstances().Result;

            foreach (ListAllServiceInstancesResponse serviceInstanceDetails in serviceInstances)
            {
                if (serviceInstanceDetails.ServicePlanGuid.HasValue)
                {
                    if (serviceInstanceDetails.Name == serviceName && serviceInstanceDetails.ServicePlanGuid.Value == planGuid)
                    {
                        return serviceInstanceDetails.EntityMetadata.Guid.ToNullableGuid();
                    }
                }
            }

            return null;
        }

        internal static Guid? GetSpaceGuid(CloudFoundryClient client, TaskLogger logger, string cforganization, string cfspace)
        {
            Guid? spaceGuid = null;
            PagedResponseCollection<ListAllOrganizationsResponse> orgList = client.Organizations.ListAllOrganizations(new RequestOptions() { Query = "name:" + cforganization }).Result;

            if (orgList.Count() > 1)
            {
                logger.LogError("There are more than one organization with name {0}, organization names need to be unique", cforganization);
                return null;
            }

            ListAllOrganizationsResponse orgInfo = orgList.FirstOrDefault();
            if (orgInfo != null)
            {
                PagedResponseCollection<ListAllSpacesForOrganizationResponse> spaceList = client.Organizations.ListAllSpacesForOrganization(orgInfo.EntityMetadata.Guid.ToNullableGuid(), new RequestOptions() { Query = "name:" + cfspace }).Result;

                if (spaceList.Count() > 1)
                {
                    logger.LogError("There are more than one space with name {0} in organization {1}", cfspace, cforganization);
                    return null;
                }

                if (spaceList.FirstOrDefault() != null)
                {
                    spaceGuid = new Guid(spaceList.FirstOrDefault().EntityMetadata.Guid);
                }
                else
                {
                    logger.LogError("Space {0} not found", cfspace);
                    return null;
                }
            }
            else
            {
                logger.LogError("Organization {0} not found", cforganization);
                return null;
            }

            return spaceGuid;
        }
    }
}
