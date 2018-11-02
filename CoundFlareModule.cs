using Abp.AutoMapper;
using Abp.Dependency;
using Abp.Modules;
using Castle.MicroKernel.Registration;
using CoundFlareTools.CoundFlare;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CoundFlareTools
{
    [DependsOn(
typeof(AbpAutoMapperModule)
    )]
    public class CoundFlareModule : AbpModule
    {
        public override void PreInitialize()
        {
            //Configuration.ReplaceService(typeof(IFrameworkDbSessionConfiguration), () =>
            //{
            //    IocManager.IocContainer.Register(
            //        Component.For<IFrameworkDbSessionConfiguration>()
            //            .ImplementedBy<BotUpdateDbSessionConfiguration>()
            //            .LifestyleTransient()
            //        );
            //});

            IocManager.IocContainer.Register(
                Component.For<Form1>(),
                Component.For<Form2>(),
                Component.For<Form3>(), 
                //Component.For<ILogsController>().ImplementedBy<LogsController>().LifestyleTransient(),
                Component.For<ILogsController>().ImplementedBy<LogsControllerImpByLog>().LifestyleTransient(),
                Component.For<ICloudflareLogHandleSercie>().ImplementedBy<CloudflareLogHandleSercie>().LifestyleTransient(),
                Component.For<ICloundFlareApiService>().ImplementedBy<CloundFlareApiService>().LifestyleTransient(),
                Classes.FromThisAssembly().Pick()
                .If(t => t.Name.EndsWith("Service") && !t.Name.Contains("LogService"))
                .WithService.DefaultInterfaces());
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(Assembly.GetExecutingAssembly());

            Configuration.Modules.AbpAutoMapper().Configurators.Add(cfg =>
            {
                //cfg.CreateMap<UserDto, User>();
                //cfg.CreateMap<UserDto, User>().ForMember(x => x.Roles, opt => opt.Ignore());
                //cfg.CreateMap<DataRow, IntentImportDto>();
                cfg.AddProfiles("CoundFlareTools");
            });

        }

    }
}
