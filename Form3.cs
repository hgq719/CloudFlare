﻿using AutoMapper;
using Castle.Core.Logging;
using CoundFlareTools.CoundFlare;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
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

        public Form2 form2 { get; set; }

        ILogger logger = Abp.Logging.LogHelper.Logger;
        public ICloudflareLogHandleSercie cloudflareLogHandleSercie { get; set; }
        private bool autoRun = false;
        private bool autoBan = false;
        private bool showAutoRunCheckBox = false;
        private bool showAutoBanCheckBox = false;
        private bool showUnbanButton = false;
        private void Form3_Load(object sender, EventArgs e)
        {
            cloudflareLogHandleSercie.SubscribeMessageEvent(Notification_Message);
            showAutoBanCheckBox = Convert.ToBoolean(ConfigurationManager.AppSettings["showAutoBanCheckBox"]);
            showUnbanButton = Convert.ToBoolean(ConfigurationManager.AppSettings["showUnbanButton"]);

            this.checkBoxAutoRun.Visible = showAutoRunCheckBox;
            this.checkBoxAutoBan.Visible = showAutoBanCheckBox;
            this.buttonUnban.Visible = showUnbanButton;
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
            doWork();
        }
        private void doWork()
        {
            do
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
                            var order = cloudflareLogReport?.CloudflareLogReportItems?.Where(a => a.Ban).OrderByDescending(a => a.Count).ToArray();
                            dataGridViewReport.DataSource = order;
                            dataGridViewReport.Refresh();
                        }));
                    }
                    else
                    {
                        button1.Enabled = true;
                    }

                    if (autoBan == true)
                    {
                        List<string> ips = new List<string>();
                        var items = cloudflareLogReport?.CloudflareLogReportItems?.Where(a => a.Ban).OrderByDescending(a => a.Count).ToArray();
                        if (items != null)
                        {
                            foreach (CloudflareLogReportItem item in items)
                            {
                                if (item.Ban)
                                {
                                    ips.Add(item.ClientIP);
                                }
                            }
                        }
                        cloudflareLogHandleSercie.BanIps(ips);
                    }

                    if (autoRun == true)
                    {
                        //去最近5分钟进行重新的分析       
                        if (this.InvokeRequired)
                        {
                            this.Invoke(new Action(() =>
                            {
                                DateTime now = Convert.ToDateTime( DateTime.Now.ToString("yyyy-MM-dd HH:mm:00"));
                                dateTimePickerStart.Value = now.AddMinutes(-10);
                                dateTimePickerEnd.Value = now.AddMinutes(-5);

                                startTime = dateTimePickerStart.Value;
                                endTime = dateTimePickerEnd.Value;
                            }));
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message);
                }
            }
            while (autoRun);
           
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

            startTime = Convert.ToDateTime( dateTimePickerStart.Value.ToString("yyyy-MM-dd HH:mm:00"));
            endTime = Convert.ToDateTime(dateTimePickerEnd.Value.ToString("yyyy-MM-dd HH:mm:00")); 

            return true;
        }

        private void checkBoxAutoRun_CheckedChanged(object sender, EventArgs e)
        {
            this.autoRun = checkBoxAutoRun.Checked;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            bool isBan = false;
            if (checkBox1.Checked)
            {
                isBan = true;
            }
            CloudflareLogReportItem[] items = dataGridViewReport.DataSource as CloudflareLogReportItem[];
            foreach (CloudflareLogReportItem item in items)
            {
                item.Ban = isBan;
            }

            dataGridViewReport.DataSource = items;
            dataGridViewReport.Refresh();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            List<string> ips = new List<string>();
            CloudflareLogReportItem[] items = dataGridViewReport.DataSource as CloudflareLogReportItem[];
            if (items != null)
            {
                foreach(CloudflareLogReportItem item in items)
                {
                    if (item.Ban)
                    {
                        ips.Add(item.ClientIP);
                    }
                }
            }

            DialogResult dialogResult = MessageBox.Show("Ban？", "warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (dialogResult == DialogResult.Yes)
            {
                cloudflareLogHandleSercie.BanIps(ips);
                MessageBox.Show("Ban Success");
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            form2.ShowDialog();
        }

        private void checkBoxAutoBan_CheckedChanged(object sender, EventArgs e)
        {
            this.autoBan = checkBoxAutoBan.Checked;
        }
    }
}
