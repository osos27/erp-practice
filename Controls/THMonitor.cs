using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Controls
{
    public partial class THMonitor : UserControl
    {
        private int titleNum;
        public int TitleNum
        {
            set 
            {
                this.titleNum = value;
                this.lblTitle.Text = value.ToString()+"号站点 "; 
            }
            get 
            {
                return this.titleNum;
            }
        }

        /// <summary>
        /// 温度警告值
        /// </summary>
        public int TemperatureWarning{get;set;}=40;
        /// <summary>
        /// 湿度警告值
        /// </summary>
        public int MoistWarning{get;set;}=45;
        //界面控件中的高度
        private int tempBarHeight=98;
        private int moistBarHeight=98;

        private int tempValue=0;
        private int moistValue=0;

        public int TemperatureValue
        {
            set 
            {
                if (value < 0 || value > 100)
                {
                    MessageBox.Show("温度值必须在0到100之间!","信息提示");
                }
                else
                {
                    this.tempValue = value;
                    //实际温度条高度
                     double actualHeight = tempBarHeight/100.0*value;
                     //遮罩高度
                     int showValue= (int)(tempBarHeight-actualHeight);
                     //设置遮罩高度
                     this.imgBar.Height = showValue;
                     this.lblTemp.Text = value.ToString() + "℃";
                     //判断是否超过警告值
                     if (value >= TemperatureWarning)
                     {
                         this.lblTemp.BackColor = Color.Red;
                     }
                     else
                     {
                         this.lblTemp.BackColor = Color.FromArgb(192, 192, 0);
                     }
                }
            }
            get 
            {
                return this.tempValue;
            }
        }

        public int MoistValue
        {
            get{
                return this.moistValue;
            }
            set{
                if (value < 0 || value > 100)
                {
                    MessageBox.Show("湿度值必须在0到100之间!","信息提示");
                }
                else
                {
                    this.moistValue = value;
                    //实际湿度条高度
                     double actualHeight = moistBarHeight/100.0*value;
                     //遮罩高度
                     int showValue= (int)(moistBarHeight-actualHeight);
                     //设置遮罩高度
                     this.imgMoistBar.Height = showValue;
                     this.lblMoist.Text = value.ToString() + "RH";
                     //判断是否超过警告值
                     if (value >= MoistWarning)
                     {
                         this.lblMoist.BackColor = Color.Red;
                     }
                     else
                     {
                         this.lblMoist.BackColor = Color.FromArgb(192, 192, 0);
                     }
                }
            }
        }



        public THMonitor()
        {
            InitializeComponent();
            this.lblTemp.Text = "0℃";
            this.lblMoist.Text = "0RH";
        }

        


        


    }
}
