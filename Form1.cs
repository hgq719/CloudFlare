using CoundFlareTools.Core;
using CoundFlareTools.CoundFlare;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CoundFlareTools
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public ICloundFlareApiService cloundFlareApiService { get; set; }
        public ILogsController logsController { get; set; }
        public IRequestlimitconfigAppService requestlimitconfigAppService { get; set; }

        private void button1_Click(object sender, EventArgs e)
        {
            DateTime start = new DateTime(2018, 10, 30, 17, 40, 43);
            DateTime end = new DateTime(2018, 10, 30, 17, 40, 44);
            bool retry = false;
            List<CloudflareLog> CloudflareLogs = cloundFlareApiService.GetCloudflareLogs(start, end, out retry);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            List<RequestLimitConfig> requestLimitConfigs = new List<RequestLimitConfig> {
                new RequestLimitConfig{
                    Url="livechathandler3.ashx",
                    Interval=2,
                    LimitTimes=30
                },
                new RequestLimitConfig{
                    Url="chatWindow.aspx",
                    Interval=1,
                    LimitTimes=10
                },
                new RequestLimitConfig{
                    Url="visitor.ashx",
                    Interval=1,
                    LimitTimes=5
                }
            };

            ConcurrentBag<CloudflareLogReport> cloudflareLogReports = new ConcurrentBag<CloudflareLogReport>();
            cloudflareLogReports.Add(new CloudflareLogReport {
                CloudflareLogReportItems=new CloudflareLogReportItem[] {
                    new CloudflareLogReportItem{
                        ClientIP="202.14.120.156",
                        ClientRequestHost="chatserver.comm100.com",
                        ClientRequestURI="chatWindow.aspx",
                        Count=10
                    },
                    new CloudflareLogReportItem{
                        ClientIP="183.182.114.50",
                        ClientRequestHost="chatserver.comm100.com",
                        ClientRequestURI="chatWindow.aspx",
                        Count=10
                    }
                }
            });
            cloudflareLogReports.Add(new CloudflareLogReport
            {
                CloudflareLogReportItems = new CloudflareLogReportItem[] {
                    new CloudflareLogReportItem{
                        ClientIP="202.14.120.156",
                        ClientRequestHost="chatserver.comm100.com",
                        ClientRequestURI="chatWindow.aspx",
                        Count=120
                    },
                    new CloudflareLogReportItem{
                        ClientIP="82.199.222.69",
                        ClientRequestHost="chatserver.comm100.com",
                        ClientRequestURI="chatWindow.aspx",
                        Count=10
                    },
                    new CloudflareLogReportItem{
                        ClientIP="82.199.222.69",
                        ClientRequestHost="chatserver.comm100.com",
                        ClientRequestURI="visitor.ashx",
                        Count=10
                    }
                }
            });

            var cloudflareLogReportItemsMany = cloudflareLogReports.SelectMany(a => a.CloudflareLogReportItems).ToList();
            var cloudflareLogReportItemsManyGroup = ( from item in cloudflareLogReportItemsMany
                                                    group item by new {
                                                        item.ClientRequestHost,
                                                        item.ClientIP,
                                                        item.ClientRequestURI
                                                    } into g                                                   
                                                    select new
                                                    {
                                                        g.Key.ClientRequestHost,
                                                        g.Key.ClientIP,
                                                        g.Key.ClientRequestURI,
                                                        Count = g.Sum(p => p.Count)
                                                    } ).ToList();

            var rels = cloudflareLogReportItemsMany.GroupBy(a => new { a.ClientIP, a.ClientRequestHost, a.ClientRequestURI }).Select(g => new { g.Key.ClientRequestHost,g.Key.ClientIP,g.Key.ClientRequestURI,Count = g.Sum(c=>c.Count) }).ToList();

            var result =( from item in cloudflareLogReportItemsManyGroup
                         from config in requestLimitConfigs
                         where item.ClientRequestURI == config.Url
                               && item.Count/2 >= ( config.LimitTimes / config.Interval )
                         select item ).ToList();
        }

        private void button3_Click(object sender, EventArgs e)
        {//3bc0d0634b944c499fcd764f64f0710a
            var x = cloundFlareApiService.CreateAccessRule(new FirewallAccessRuleRequest
            {
                configuration = new Configuration
                {
                    target = "ip",
                    value = "0.0.0.0",
                },
                mode = EnumMode.challenge,
                notes = "Add By Defense System",
            });
            var xx = cloundFlareApiService.GetAccessRuleList("1", "Add By Defense System");
            var y = cloundFlareApiService.DeleteAccessRule("3bc0d0634b944c499fcd764f64f0710a");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            logsController.DeleteFirewallAccessRule("xxx");
            logsController.InsertFirewallAccessRule(new List<FirewallAccessRule>() {
                new FirewallAccessRule{
                    id = "xxx",
                    notes="xxx",
                    mode=EnumMode.block,
                    configurationTarget="ip",
                    configurationValue="0.0.0.0",
                    createTime=DateTime.Now,
                    modifiedTime=DateTime.Now,
                    scopeId="xxx",
                    scopeEmail="xxx",
                    scopeType="xxx",
                }
            });
            var xx = logsController.GetFirewallAccessRuleList();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("start？", "warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (dialogResult == DialogResult.Yes)
            {
             
            }

            Form4 frm = new Form4();
            frm.ShowDialog();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            var xx = cloundFlareApiService.GetRateLimitRuleList();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            var xx = requestlimitconfigAppService.GetAll();
        }
    }
}
