using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.AutoMapper;
using Abp.Application.Services.Dto;
using CoundFlareTools.Core.Domain.Model;

namespace CoundFlareTools.Core
{
    [AutoMap(typeof(Triggerlogs))]
    public class TriggerlogsDto : EntityDto<int>
    {
        public int? RequestLimitConfigId { get; set; }   
        public string RequestLimitConfigDetail { get; set; }   
        public int? IpNumber { get; set; }   
        public DateTime? TriggerTime { get; set; }   
        public string Action { get; set; }   
        public string Remark { get; set; }   
    }
}

