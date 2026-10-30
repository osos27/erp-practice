using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace tempWork
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        int value = 0;
        private void btnAdd_Click(object sender, EventArgs e)
        {
            value++;
            this.th.TemperatureValue=value;
            this.th.MoistValue = value;
        }

        private void btnDown_Click(object sender, EventArgs e)
        {
            value--;
            this.th.TemperatureValue = value;
            this.th.MoistValue = value;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
