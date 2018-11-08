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
    [AutoMap(typeof(Settings))]
    public class SettingsDto : EntityDto<int>
    {
        public string Key { get; set; }   
        public string Value { get; set; }
        public string Remark { get; set; }
    }
}

