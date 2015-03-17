using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace CloudFoundry.Build.Tasks
{
    internal static class Utils
    {
        internal static PushProperties DeserializeFromFile(String FilePath)
        {
            PushProperties returnValue;
            Deserializer deserializer = new Deserializer();
            string content = File.ReadAllText(FilePath).Replace("app-dir", "app_dir").Replace("placement-zone", "placement_zone").Replace("sso-enabled", "sso_enabled");
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

            using (TextWriter writer = new StringWriter(builder))
            {
                serializer.Serialize(writer, ConfigurationParameters);
            }

            File.WriteAllText(FilePath, builder.ToString().Replace("app_dir", "app-dir").Replace("placement_zone", "placement-zone").Replace("sso_enabled", "sso-enabled"));
        }
    }
}
