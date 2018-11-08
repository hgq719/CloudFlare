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
    [AutoMap(typeof(Triggerlogdetails))]
    public class TriggerlogdetailsDto : EntityDto<int>
    {
        public int TriggerLogId { get; set; }
        public DateTime? StartTime { get; set; }   
        public DateTime? EndTime { get; set; }   
        public int? Size { get; set; }   
        public double? Sample { get; set; }   
        public string ClientIP { get; set; }   
        public string ClientRequestHost { get; set; }   
        public string ClientRequestURI { get; set; }   
        public int? Count { get; set; }   
    }
}

