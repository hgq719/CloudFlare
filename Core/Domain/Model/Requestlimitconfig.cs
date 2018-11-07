using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoundFlareTools.Core.Domain.Model
{
   public  class Requestlimitconfig : Entity<int>
    {
        public string Url { get; set; }   
        public int? Interval { get; set; }   
        public int? LimitTimes { get; set; }   
        public string Remark { get; set; }   
        public DateTime? CreateTime { get; set; }   
        public bool Status { get; set; }   
    }
}
