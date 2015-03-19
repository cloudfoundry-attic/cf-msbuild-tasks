using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace CloudFoundry.Build.Tasks
{
    public class AppDetails
    {
        public string name { get; set; }
        public string url { get; set; }
    }

    public class Cpu
    {
        public int max { get; set; }
        public int min { get; set; }
    }

    public class Instances
    {
        public int max { get; set; }
        public int min { get; set; }
    }

    public class Autoscale
    {
        public Cpu cpu { get; set; }
        public string enabled { get; set; }
        public Instances instances { get; set; }
    }

    public class ServiceDetails
    {
        public string plan { get; set; }
        public string type { get; set; }
    }

    public class PushProperties
    {
        [YamlMember(Alias = "app-dir")]
        public string app_dir { get; set; }
        public Dictionary<string, AppDetails> applications { get; set; }
        public Autoscale autoscale { get; set; }
        public int disk { get; set; }
        public int memory { get; set; }
        public int instances { get; set; }
        public string name { get; set; }
     
        [YamlMember(Alias = "placement-zone")]
        public string placement_zone { get; set; }
        public Dictionary<string, ServiceDetails> services { get; set; }

        [YamlMember(Alias = "sso-enabled")]
        public string sso_enabled { get; set; }
        public string stack { get; set; }
    }

}
