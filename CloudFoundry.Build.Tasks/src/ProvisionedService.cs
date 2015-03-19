using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CloudFoundry.Build
{
    public class ProvisionedService
    {
        private string name;
        private string plan;
        private string type;
        
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public string Plan
        {
            get { return plan; }
            set { plan = value; }
        }
        
        public string Type
        {
            get { return type; }
            set { type = value; }
        }
    }
}
