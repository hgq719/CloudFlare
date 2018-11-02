using AutoMapper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoundFlareTools
{
    public class AutoMappingConfig : Profile
    {
        public AutoMappingConfig()
        {
            //CreateMap<UpgradeReport, UpgradeReportExcel>()
            // .ForMember(dest => dest.SiteId, opt => opt.MapFrom(src => src.SiteId))
            // .ForMember(dest => dest.BotId, opt => opt.MapFrom(src => src.BotId))
            // .ForMember(dest => dest.UpgradeType, opt => opt.MapFrom(src => src.UpgradeType))
            // .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.status == UpgradeStatus.Succeed ? "Success" : "Fail"));

        }
    }
}
