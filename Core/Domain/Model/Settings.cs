using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoundFlareTools.Core.Domain.Model
{
   public  class Settings : Entity<int>
    {
        public string Key { get; set; }   
        public string Value { get; set; }
        public string Remark { get; set; }
    }
}
