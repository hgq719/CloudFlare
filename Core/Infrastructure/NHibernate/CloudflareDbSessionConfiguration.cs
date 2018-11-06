using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using Framwork.Infrastructure;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CoundFlareTools.Core.Infrastructure.NHibernate
{
    public class CloudflareDbSessionConfiguration: FrameworkDbSessionConfiguration
    {
        public override ISessionFactory SessionFactory { get; }
        public CloudflareDbSessionConfiguration()
           : base()
        {
            FluentConfiguration = Fluently.Configure();
            // 数据库连接串
            var connString = "data source=|DataDirectory|Cloudflare.db;";
            FluentConfiguration
                // 配置连接串
                .Database(SQLiteConfiguration.Standard.ConnectionString(connString))
                // 配置ORM
                .Mappings(m => m.FluentMappings.AddFromAssembly(Assembly.GetExecutingAssembly()));
            // 生成session factory
            SessionFactory = FluentConfiguration.BuildSessionFactory();
        }
    }
}
