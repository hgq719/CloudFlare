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
    public partial class Form8 : Form
    {
        public Form8()
        {
            InitializeComponent();
        }
        public List<CloudflareLog> CloudflareLogs { get; set; }
        private void Form8_Load(object sender, EventArgs e)
        {
            var data = CloudflareLogs.OrderBy(a => a.ClientRequestHost).ThenBy(a => a.ClientIP).ToList();
            dataGridView1.DataSource = data;
            dataGridView1.Refresh();
            labelTotal.Text = string.Format("total:{0}", data.Count());

            List<string> filterStringList = new List<string>() { "all" };
            filterStringList.AddRange(CloudflareLogs.Select(a => a.ClientRequestHost).Distinct());

            comboBoxHost.DataSource = filterStringList;
        }

        private void comboBoxHost_SelectedIndexChanged(object sender, EventArgs e)
        {
            string filterString = comboBoxHost.Text;
            var order = CloudflareLogs.OrderBy(a => a.ClientRequestHost).ThenBy(a => a.ClientIP).ToList();
            if (filterString == "all")
            {

            }
            else
            {
                order = order.Where(a => a.ClientRequestHost == filterString).ToList();
            }

            dataGridView1.DataSource = order;
            dataGridView1.Refresh();
            labelTotal.Text = string.Format("total:{0}", order.Count());
        }

        private void textBoxIp_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
