using FluentNHibernate.Mapping;
using CoundFlareTools.Core.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoundFlareTools.Core
{
    public class SettingsMapper : ClassMap<Settings>
    {
        public SettingsMapper()
        {
            // 禁用惰性加载
            Not.LazyLoad();
            // 映射到表tweet
            Table("t_Cloudflare_Settings");
            // 主键映射
            Id(x => x.Id).Column("Id");
            // 字段映射
            Map(x => x.Key).Column("[Key]");
            Map(x => x.Value).Column("[Value]");
            Map(x => x.Remark).Column("Remark");
        }
    }
}