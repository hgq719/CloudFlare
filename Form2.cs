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
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }
        public ICloundFlareApiService cloundFlareApiService { get; set; }
        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Unban？", "warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (dialogResult == DialogResult.Yes)
            {
                string text = richTextBox1.Text;
                string[] ips = text.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
                List<FirewallAccessRule> firewallAccessRules = cloundFlareApiService.GetAccessRuleList("", "Add By Defense System");
                foreach (string ip in ips)
                {
                    FirewallAccessRule firewallAccessRule = firewallAccessRules.FirstOrDefault(a => a.configurationValue == ip);
                    if (firewallAccessRule != null && !string.IsNullOrEmpty(firewallAccessRule.id))
                    {
                        cloundFlareApiService.DeleteAccessRule(firewallAccessRule.id);
                    }
                }

                MessageBox.Show("Unban Success");
            }

             
        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }
    }
}
