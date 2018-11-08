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
    public partial class Form7 : Form
    {
        public Form7()
        {
            InitializeComponent();
        }
        public IRequestlimitconfigAppService requestlimitconfigAppService { get; set; }
        public int Id { get; set; }
        private void button1_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBoxUrl.Text) &&
                !string.IsNullOrEmpty(textBoxInterval.Text)&&
                !string.IsNullOrEmpty(textBoxLimitTimes.Text))
            {
                if (Id > 0)
                {
                    var data = requestlimitconfigAppService.Get(new Abp.Application.Services.Dto.EntityDto<int>
                    {
                        Id = Id
                    });

                    data.Url = textBoxUrl.Text;
                    data.Interval =Convert.ToInt32( textBoxInterval.Text);
                    data.LimitTimes = Convert.ToInt32(textBoxLimitTimes.Text);
                    data.Status = checkBoxStatus.Checked;
                    data.Remark = textBoxRemark.Text;

                    requestlimitconfigAppService.Update(data);
                }
                else
                {
                    var data = new RequestlimitconfigDto
                    {
                        Url = textBoxUrl.Text,
                        Interval = Convert.ToInt32(textBoxInterval.Text),
                        LimitTimes = Convert.ToInt32(textBoxLimitTimes.Text),
                        Status = checkBoxStatus.Checked,
                        CreateTime=DateTime.Now,
                        Remark= textBoxRemark.Text,                        
                    };

                    requestlimitconfigAppService.Create(data);
                }

                MessageBox.Show("update success");
            }
        }

        private void Form7_Load(object sender, EventArgs e)
        {
            if (Id > 0)
            {
                var data = requestlimitconfigAppService.Get(new Abp.Application.Services.Dto.EntityDto<int>
                {
                    Id = Id
                });

                if (data != null && data.Id > 0)
                {
                    textBoxUrl.Text = data.Url;
                    textBoxInterval.Text = data.Interval.ToString();
                    textBoxLimitTimes.Text = data.LimitTimes.ToString();
                    checkBoxStatus.Checked = Convert.ToBoolean(data.Status);
                    textBoxRemark.Text = data.Remark;
                }
            }
            else
            {
                textBoxUrl.Text = "";
                textBoxInterval.Text = "";
                textBoxLimitTimes.Text = "";
                checkBoxStatus.Checked = true;
                textBoxRemark.Text = "";
            }
        }
    }
}
