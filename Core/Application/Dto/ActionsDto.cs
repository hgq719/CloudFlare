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
    [AutoMap(typeof(Actions))]
    public class ActionsDto : EntityDto<int>
    {
        public int? TriggerLogId { get; set; }   
        public string ClientIP { get; set; }   
        public DateTime? ActionTime { get; set; }   
        public string Mode { get; set; }   
        public string Remark { get; set; }   
    }
}

