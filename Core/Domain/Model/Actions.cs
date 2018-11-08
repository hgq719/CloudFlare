using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoundFlareTools.Core.Domain.Model
{
   public  class Actions : Entity<int>
    {
        public int? TriggerLogId { get; set; }   
        public string ClientIP { get; set; }   
        public DateTime? ActionTime { get; set; }   
        public string Mode { get; set; }   
        public string Remark { get; set; }   
    }
}
