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
    [AutoMap(typeof(Requestlimitconfig))]
    public class RequestlimitconfigDto : EntityDto<int>
    {
        public string Url { get; set; }   
        public int? Interval { get; set; }   
        public int? LimitTimes { get; set; }   
        public string Remark { get; set; }   
        public DateTime? CreateTime { get; set; }   
        public bool Status { get; set; }   
    }
}

