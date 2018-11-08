using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoundFlareTools.Core.Domain.Model
{
   public  class Triggerlogs : Entity<int>
    {
        public int? RequestLimitConfigId { get; set; }   
        public string RequestLimitConfigDetail { get; set; }   
        public int? IpNumber { get; set; }   
        public DateTime? TriggerTime { get; set; }   
        public string Action { get; set; }   
        public string Remark { get; set; }   
    }
}
