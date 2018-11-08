using FluentNHibernate.Mapping;
using CoundFlareTools.Core.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoundFlareTools.Core
{
    public class ActionsMapper : ClassMap<Actions>
    {
        public ActionsMapper()
        {
            // 禁用惰性加载
            Not.LazyLoad();
            // 映射到表tweet
            Table("t_Cloudflare_Actions");
            // 主键映射
            Id(x => x.Id).Column("Id");
            // 字段映射
            Map(x => x.TriggerLogId).Column("TriggerLogId");
            Map(x => x.ClientIP).Column("ClientIP");
            Map(x => x.ActionTime).Column("ActionTime");
            Map(x => x.Mode).Column("Mode");
            Map(x => x.Remark).Column("Remark");
        }
    }
}