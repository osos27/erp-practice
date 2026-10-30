﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Controls;
using Modbus.Device;
using Models;



namespace tempWork
{
    public partial class FrmMain : Form
    {
        /// <summary>
        /// 私有串口
        /// </summary>
        ModbusRTU modbus=new ModbusRTU();
        private Dictionary<byte,THMonitorDB> currentValue = new Dictionary<byte, THMonitorDB>();
        /// <summary>
        /// 定时器
        /// </summary>
        private System.Windows.Forms.Timer updateTimer = new System.Windows.Forms.Timer();
        //取消线程源
        private CancellationTokenSource cts;
        private Point mPoint;
        private Config config;
        public FrmMain()
        {
            InitializeComponent();
            Init();
        }

        public void Init()
        {
            colorZone.MouseDown += ColorZone_MouseDown;
            colorZone.MouseMove += ColorZone_MouseMove;

            BindClickToControl(btnExit, BtnExit_Click);

            config = new Config()
            {
                portName = "COM19",
                baudRate = 9600,
                Parity = Parity.None,
                DataBits = 8,
                StopBits = StopBits.One,
                slaveId1 = 1,
                slaveId2 = 2,
                slaveId3 = 3,
                slaveId4 = 4,
            };
            bool result = modbus.Connect(config.portName, config.baudRate, config.Parity, config.DataBits, config.StopBits);
            if (!result)
            {
                MessageBox.Show("连接失败");
                return;
            }
            cts = new CancellationTokenSource();
            //通信
            Task.Run(new Action(() =>
            {
                
                Communicate();
            }),cts.Token);

            updateTimer.Interval = 200;
            updateTimer.Tick += UpdateTimer_Tick;
            updateTimer.Start();

        }
        public void UpdateTimer_Tick(object sender, EventArgs e)
        {
            if(currentValue.ContainsKey(config.slaveId1))
            {
                THMonitorDB temp = currentValue[config.slaveId1];
                this.th1.TemperatureValue = temp.TemperatureValue;
                this.th1.MoistValue = temp.MoistureValue;
            }
            if(currentValue.ContainsKey(config.slaveId2))
            {
                THMonitorDB temp = currentValue[config.slaveId2];
                this.th2.TemperatureValue = temp.TemperatureValue;
                this.th2.MoistValue = temp.MoistureValue;
            }
            if(currentValue.ContainsKey(config.slaveId3))
            {
                THMonitorDB temp = currentValue[config.slaveId3];
                this.th3.TemperatureValue = temp.TemperatureValue;
                this.th3.MoistValue = temp.MoistureValue;
            }
            if(currentValue.ContainsKey(config.slaveId4))
            {
                THMonitorDB temp = currentValue[config.slaveId4];
                this.th4.TemperatureValue = temp.TemperatureValue;
                this.th4.MoistValue = temp.MoistureValue;
            }
        }

        private bool GetSlaveData(byte slaveId)
        {
            byte[] res = modbus.ReadInputRegister(slaveId, 1, 2);
            if (res != null)
            {
                    //温度解析
                    float temp = (res[0] * 256 + res[1]) * 0.1f;
                    float humity = (res[2] * 256 + res[3]) * 0.1f;
                    if (currentValue.ContainsKey(slaveId))
                    {
                        currentValue[slaveId] = new THMonitorDB() { TemperatureValue = (int)temp, MoistureValue = (int)humity };
                    }
                    else
                    {
                        currentValue.Add(slaveId, new THMonitorDB() { TemperatureValue = (int)temp, MoistureValue = (int)humity });
                    }
                    return true;
            }
            else
            {
                MessageBox.Show($"读取从站{slaveId}数据失败");
                return false;
            }
        }

        private void Communicate()
        {
            while(!cts.Token.IsCancellationRequested)
            {
                //四个从站
                // 通信逻辑
                GetSlaveData(config.slaveId1);
                GetSlaveData(config.slaveId2);
                GetSlaveData(config.slaveId3);
                GetSlaveData(config.slaveId4);
            }
        }

        private void BindClickToControl(Control control, EventHandler clickHandler)
        {
            control.Click += clickHandler;
            foreach (Control child in control.Controls)
            {
                BindClickToControl(child, clickHandler);
            }
        }

        private void BtnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ColorZone_MouseDown(object sender, MouseEventArgs e)
        {
            mPoint = e.Location;
        }

        private void ColorZone_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.Location = new Point(this.Location.X + e.X - mPoint.X, this.Location.Y + e.Y - mPoint.Y);
            }
        }

        private void FrmMain_FormClosed(object sender, FormClosedEventArgs e)
        {  
            cts?.Cancel();
        }
    }
}