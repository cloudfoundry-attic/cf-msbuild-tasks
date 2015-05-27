namespace CloudFoundry.Build.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class ProvisionedService
    {
        private string name;
        private string plan;
        private string type;

        public string Name
        {
            get { return this.name; }
            set { this.name = value; }
        }

        public string Plan
        {
            get { return this.plan; }
            set { this.plan = value; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Justification = "Based on default cf yaml format")]
        public string Type
        {
            get { return this.type; }
            set { this.type = value; }
        }
    }
}
