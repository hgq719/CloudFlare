using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoundFlareTools.CoundFlare
{
    public class RequestLimitConfig
    {
        public int Id { get; set; }

        public string Url { get; set; }

        public int Interval { get; set; }

        public int LimitTimes { get; set; }

        public string Remark { get; set; }
    }
    public class Config
    {
        public int TriggerRatio { get; set; }
        public int TriggerCreateNumber { get; set; }
        public int TriggerDeleteTime { get; set; }

        public List<RequestLimitConfig> RateLimits { get; set; }

    }
}
