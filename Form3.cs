using AutoMapper;
using Castle.Core.Logging;
using CoundFlareTools.CoundFlare;
using Newtonsoft.Json;
using System;
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
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();
        }

        ILogger logger = Abp.Logging.LogHelper.Logger;
        public ICloudflareLogHandleSercie cloudflareLogHandleSercie { get; set; }

        private void Form3_Load(object sender, EventArgs e)
        {
            cloudflareLogHandleSercie.SubscribeMessageEvent(Notification_Message);
            //CloudflareLogReport cloudflareLogReport = new CloudflareLogReport {
            //     CloudflareLogReportItems = new CloudflareLogReportItem[] {
            //         new CloudflareLogReportItem{
            //             ClientRequestHost="xx",
            //             ClientIP="xxxxx",
            //             ClientRequestURI="xxxx",
            //             Count=10,
            //             Ban=true
            //         }
            //     }
            //};
            //var order = cloudflareLogReport.CloudflareLogReportItems.Where(a => a.Ban).OrderByDescending(a => a.Count).ToArray();
            //dataGridViewReport.DataSource = order;

        }
        private void Notification_Message(object sender, MessageEventArgs e)
        {
            if (richTextBoxMessage.InvokeRequired)
            {
                richTextBoxMessage.Invoke(new Action(() =>
                {
                    if (richTextBoxMessage.TextLength > 10000)
                    {
                        richTextBoxMessage.Text = "";
                    }
                    richTextBoxMessage.AppendText(string.Format("{0}:{1}", DateTime.Now.ToString("hh:mm:ss"), e.Message + Environment.NewLine));
                }));
            }
            else
            {
                if (richTextBoxMessage.TextLength > 1000)
                {
                    richTextBoxMessage.Text = "";
                }
                richTextBoxMessage.AppendText(string.Format("{0}:{1}", DateTime.Now.ToString("hh:mm:ss"), e.Message + Environment.NewLine));
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            DialogResult dialogResult = MessageBox.Show("start？", "warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (dialogResult == DialogResult.Yes)
            {
                if (Check())
                {
                    backgroundWorker1.RunWorkerAsync();
                }
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {

                cloudflareLogHandleSercie.InitQueue(startTime, endTime);
                cloudflareLogHandleSercie.TaskStart();
                CloudflareLogReport cloudflareLogReport = cloudflareLogHandleSercie.GetCloudflareLogReport();

                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() =>
                    {
                        button1.Enabled = true;
                        var order = cloudflareLogReport.CloudflareLogReportItems.Where(a => a.Ban).OrderByDescending(a => a.Count).ToArray();
                        dataGridViewReport.DataSource = order;

                    }));
                }
                else
                {
                    button1.Enabled = true;
                }
            }
            catch(Exception ex)
            {
                logger.Error(ex.Message);
            }

        }

        private DateTime startTime;
        private DateTime endTime;
        private bool Check()
        {
            if (string.IsNullOrEmpty(dateTimePickerStart.Text))
            {
                MessageBox.Show("start time require");
                return false;
            }
            if (string.IsNullOrEmpty(dateTimePickerEnd.Text))
            {
                MessageBox.Show("end time require");
                return false;
            }

            startTime = dateTimePickerStart.Value;
            endTime = dateTimePickerEnd.Value;

            return true;
        }
    }
}
