using Abp;
using Abp.Castle.Logging.Log4Net;
using Castle.Facilities.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CoundFlareTools
{
    static class Program
    {
        static AbpBootstrapper bootstrapper;

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
            //SetDataDirectory();
            Form3 frm = bootstrapper.IocManager.Resolve<Form3>();
            Application.Run(frm);
        }

        static void AbpBootStrapper()
        {
            bootstrapper = AbpBootstrapper.Create<CoundFlareModule>();
            bootstrapper.IocManager.IocContainer
                .AddFacility<LoggingFacility>(f => f.UseAbpLog4Net()
                    .WithConfig("log4net.config")
                );
            bootstrapper.Initialize();
        }
    }
}
