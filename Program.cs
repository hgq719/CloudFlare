using Abp;
using Abp.Castle.Logging.Log4Net;
using Castle.Facilities.Logging;
using System;
using System.Windows.Forms;

namespace CoundFlareTools
{
    static class Program
    {
        static AbpBootstrapper _bootstrapper;

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            //Comment
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            AbpBootStrapper();
            SetDataDirectory();
            var frm = _bootstrapper.IocManager.Resolve<Form3>();
            Application.Run(frm);
        }

        static void AbpBootStrapper()
        {
            _bootstrapper = AbpBootstrapper.Create<CoundFlareModule>();
            _bootstrapper.IocManager.IocContainer
                .AddFacility<LoggingFacility>(f => f.UseAbpLog4Net()
                    .WithConfig("log4net.config")
                );
            _bootstrapper.Initialize();
        }
        static void SetDataDirectory()
        {
            var dataDir = AppDomain.CurrentDomain.BaseDirectory;
            dataDir = $"{dataDir}CoundFlare";
            AppDomain.CurrentDomain.SetData("DataDirectory", dataDir);
        }
    }
}
