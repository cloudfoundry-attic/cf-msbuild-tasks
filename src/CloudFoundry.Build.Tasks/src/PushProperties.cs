namespace CloudFoundry.Build.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using YamlDotNet.Serialization;

    public class AppDetails
    {
        [YamlMember(Alias = "name")]
        public string Name { get; set; }

        [YamlMember(Alias = "url")]
        public string Url { get; set; }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Reviewed.")]
    public class Cpu
    {
        [YamlMember(Alias = "max")]
        public int MaxCpu { get; set; }

        [YamlMember(Alias = "min")]
        public int MinCpu { get; set; }
    }

    public class Instances
    {
        [YamlMember(Alias = "max")]
        public int MaxInstances { get; set; }
        [YamlMember(Alias = "min")]
        public int MinInstances { get; set; }
    }

    public class Autoscale
    {
        [YamlMember(Alias = "cpu")]
        public Cpu Cpu { get; set; }

        [YamlMember(Alias = "enabled")]
        public string Enabled { get; set; }

        [YamlMember(Alias = "instances")]
        public Instances InstancesInfo { get; set; }
    }

    public class ServiceDetails
    {
        [YamlMember(Alias = "plan")]
        public string Plan { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Justification = "Based on default cf yaml format"), YamlMember(Alias = "type")]
        public string Type { get; set; }
    }

    public class PushProperties
    {
        [YamlMember(Alias = "app-dir")]
        public string AppDir { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Set by deserialization"), YamlMember(Alias = "applications")]
        public Dictionary<string, AppDetails> Applications { get; set; }

        [YamlMember(Alias = "autoscale")]
        public Autoscale AutoscaleInfo { get; set; }

        [YamlMember(Alias = "disk")]
        public int Disk { get; set; }

        [YamlMember(Alias = "memory")]
        public int Memory { get; set; }

        [YamlMember(Alias = "instances")]
        public int Instances { get; set; }

        [YamlMember(Alias = "name")]
        public string Name { get; set; }

        [YamlMember(Alias = "placement-zone")]
        public string PlacementZone { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Set by deserialization"), YamlMember(Alias = "services")]
        public Dictionary<string, ServiceDetails> Services { get; set; }

        [YamlMember(Alias = "sso-enabled")]
        public string SsoEnabled { get; set; }

        [YamlMember(Alias = "stack")]
        public string Stack { get; set; }
    }
}
