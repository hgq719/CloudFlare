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
    public partial class Form6 : Form
    {
        public Form6()
        {
            InitializeComponent();
        }
        public int Id { get; set; }
        public ISettingsAppService settingsAppService { get; set; }
        private void Form6_Load(object sender, EventArgs e)
        {
            if (Id > 0)
            {
                var data = settingsAppService.Get(new Abp.Application.Services.Dto.EntityDto<int>
                {
                    Id = Id
                });

                if(data!=null&& data.Id > 0)
                {
                    textBoxKey.Text = data.Key;
                    textBoxValue.Text = data.Value;
                    textBoxRemark.Text = data.Remark;
                }
            }
            else
            {
                textBoxKey.Text = "";
                textBoxValue.Text = "";
                textBoxRemark.Text ="";
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBoxKey.Text) &&
                !string.IsNullOrEmpty(textBoxValue.Text))
            {
                if (Id > 0)
                {
                    var data = settingsAppService.Get(new Abp.Application.Services.Dto.EntityDto<int>
                    {
                        Id = Id
                    });

                    data.Key = textBoxKey.Text;
                    data.Value = textBoxValue.Text;
                    data.Remark = textBoxRemark.Text;

                    settingsAppService.Update(data);
                }
                else
                {
                    var data = new SettingsDto {
                        Key = textBoxKey.Text,
                        Value= textBoxValue.Text,
                        Remark = textBoxRemark.Text,
                    };
                    settingsAppService.Create(data);
                }

                MessageBox.Show("update success");
            }
            
        }
    }
}
