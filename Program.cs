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
            SetDataDirectory();
            Form1 frm = bootstrapper.IocManager.Resolve<Form1>();
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
        static void SetDataDirectory()
        {
            string dataDir = AppDomain.CurrentDomain.BaseDirectory;
            //dataDir = dataDir.Remove(dataDir.IndexOf(@"\bin\"));
            dataDir =  string.Format("{0}{1}", dataDir, "CoundFlare");
            AppDomain.CurrentDomain.SetData("DataDirectory", dataDir);

            var xx = AppDomain.CurrentDomain.GetData("DataDirectory");
        }
    }
}
