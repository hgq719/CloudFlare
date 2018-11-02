using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoundFlareTools.CoundFlare
{
    public class FirewallAccessRule
    {
        public string id { get; set; }
        public string notes { get; set; }
        public string mode { get; set; }
        public string configurationTarget { get; set; }
        public string configurationValue { get; set; }
        public DateTime createTime { get; set; }
        public DateTime modifiedTime { get; set; }
        public string scopeId { get; set; }
        public string scopeEmail { get; set; }
        public string scopeType { get; set; }
    }
}
