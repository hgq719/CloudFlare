using CoundFlareTools.Core;
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
    public partial class Form5 : Form
    {
        public Form5()
        {
            InitializeComponent();
        }

        public ISettingsAppService settingsAppService { get; set; }
        public IRequestlimitconfigAppService requestlimitconfigAppService { get; set; }
        public Form6 form6 { get; set; }
        public Form7 form7 { get; set; }
        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

        void loadData()
        {
            var data = settingsAppService.GetAll();
            dataGridView1.DataSource = data;
            dataGridView1.Refresh();

            var data2 = requestlimitconfigAppService.GetAll();
            dataGridView2.DataSource = data2;
            dataGridView2.Refresh();
        }

        private void Form5_Load(object sender, EventArgs e)
        {
            loadData();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            loadData();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //var data = dataGridView1.DataSource as List<SettingsDto>;
            //foreach(var item in data)
            //{
            //    settingsAppService.Update(item);
            //}        
            //MessageBox.Show("update success");
            loadData();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            form6.Id = 0;
            form6.ShowDialog();
            loadData();
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex > -1)
            {
                var id = Convert.ToInt32(dataGridView1.Rows[e.RowIndex].Cells["Id"].Value);
                form6.Id = id;
                form6.ShowDialog();
                loadData();
            }        
        }

        private void button4_Click(object sender, EventArgs e)
        {
            loadData();
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            form7.Id = 0;
            form7.ShowDialog();
            loadData();
        }

        private void tabPage2_Click(object sender, EventArgs e)
        {

        }

        private void dataGridView2_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex > -1)
            {
                var id = Convert.ToInt32(dataGridView2.Rows[e.RowIndex].Cells["Id"].Value);
                form7.Id = id;
                form7.ShowDialog();
                loadData();
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (dataGridView2.SelectedRows.Count > 0)
            {
                DialogResult dialogResult = MessageBox.Show("delete？", "warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (dialogResult == DialogResult.Yes)
                {
                    int id = Convert.ToInt32(dataGridView2.SelectedRows[0].Cells["Id"].Value);
                    requestlimitconfigAppService.Delete(new Abp.Application.Services.Dto.EntityDto<int>
                    {
                        Id = id
                    });
                    MessageBox.Show("delete success");
                    loadData();
                }
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                DialogResult dialogResult = MessageBox.Show("delete？", "warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (dialogResult == DialogResult.Yes)
                {
                    int id = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["Id"].Value);
                    settingsAppService.Delete(new Abp.Application.Services.Dto.EntityDto<int>
                    {
                        Id = id
                    });
                    MessageBox.Show("delete success");
                    loadData();
                }
            }
        }
    }
}
