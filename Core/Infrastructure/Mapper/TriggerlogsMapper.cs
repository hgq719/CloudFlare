using FluentNHibernate.Mapping;
using CoundFlareTools.Core.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoundFlareTools.Core
{
    public class TriggerlogsMapper : ClassMap<Triggerlogs>
    {
        public TriggerlogsMapper()
        {
            // 禁用惰性加载
            Not.LazyLoad();
            // 映射到表tweet
            Table("t_Cloudflare_TriggerLogs");
            // 主键映射
            Id(x => x.Id).Column("Id");
            // 字段映射
            Map(x => x.RequestLimitConfigId).Column("RequestLimitConfigId");
            Map(x => x.RequestLimitConfigDetail).Column("RequestLimitConfigDetail");
            Map(x => x.IpNumber).Column("IpNumber");
            Map(x => x.TriggerTime).Column("TriggerTime");
            Map(x => x.Action).Column("Action");
            Map(x => x.Remark).Column("Remark");
        }
    }
}