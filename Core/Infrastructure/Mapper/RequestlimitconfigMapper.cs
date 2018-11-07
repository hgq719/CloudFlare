using FluentNHibernate.Mapping;
using CoundFlareTools.Core.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoundFlareTools.Core
{
    public class RequestlimitconfigMapper : ClassMap<Requestlimitconfig>
    {
        public RequestlimitconfigMapper()
        {
            // 禁用惰性加载
            Not.LazyLoad();
            // 映射到表tweet
            Table("t_Cloudflare_RequestLimitConfig");
            // 主键映射
            Id(x => x.Id).Column("Id");
            // 字段映射
            Map(x => x.Id).Column("Id");
            Map(x => x.Url).Column("Url");
            Map(x => x.Interval).Column("Interval");
            Map(x => x.LimitTimes).Column("LimitTimes");
            Map(x => x.Remark).Column("Remark");
            Map(x => x.CreateTime).Column("CreateTime");
            Map(x => x.Status).Column("Status");
        }
    }
}