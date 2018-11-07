using CoundFlareTools.CoundFlare;
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
    public partial class Form4 : Form
    {
        public Form4()
        {
            InitializeComponent();
        }
        public ICloundFlareApiService cloundFlareApiService { get; set; }
        private void button1_Click(object sender, EventArgs e)
        {
            string ip = textBoxIp.Text;
            List<FirewallAccessRule> firewallAccessRules = new List<FirewallAccessRule>();
            if (string.IsNullOrWhiteSpace(ip))
            {
                firewallAccessRules = cloundFlareApiService.GetAccessRuleList("", "Add By Defense System");
            }
            else
            {
                firewallAccessRules = cloundFlareApiService.GetAccessRuleList(ip, "");
            }
            var orderBy = firewallAccessRules.Where(a=>a.mode == EnumMode.challenge).OrderByDescending(a => a.createTime).ToArray();
            dataGridView1.DataSource = orderBy;
            dataGridView1.Refresh();
        }
        public Form2 form2 { get; set; }
        private void button2_Click(object sender, EventArgs e)
        {
            form2.ShowDialog();
        }
    }
}
