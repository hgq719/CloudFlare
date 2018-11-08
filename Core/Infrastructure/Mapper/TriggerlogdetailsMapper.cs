using FluentNHibernate.Mapping;
using CoundFlareTools.Core.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoundFlareTools.Core
{
    public class TriggerlogdetailsMapper : ClassMap<Triggerlogdetails>
    {
        public TriggerlogdetailsMapper()
        {
            // 禁用惰性加载
            Not.LazyLoad();
            // 映射到表tweet
            Table("t_Cloudflare_TriggerLogDetails");
            // 主键映射
            Id(x => x.Id).Column("Id");
            // 字段映射
            Map(x => x.TriggerLogId).Column("TriggerLogId");
            Map(x => x.StartTime).Column("StartTime");
            Map(x => x.EndTime).Column("EndTime");
            Map(x => x.Size).Column("Size");
            Map(x => x.Sample).Column("Sample");
            Map(x => x.ClientIP).Column("ClientIP");
            Map(x => x.ClientRequestHost).Column("ClientRequestHost");
            Map(x => x.ClientRequestURI).Column("ClientRequestURI");
            Map(x => x.Count).Column("Count");
        }
    }
}