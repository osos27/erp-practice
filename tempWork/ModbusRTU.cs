using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace tempWork
{
    public class ModbusRTU
    {
        private SerialPort serialPort;
        //读取超时时间2000ms
        public int ReadTimeout { get; set; } = 2000;
        //写入超时时间2000ms
        public int WriteTimeout { get; set; } = 2000;
        //每次串口通信前延迟时间
        public int SleepTime { get; set; } = 100;
        //接收超时时间2000ms
        public int ReceiveTimeout { get; set; } = 2000;
        private bool dtrEnable = false;
        public bool DtrEnable
        {
            get { return dtrEnable; }
            set
            {
                dtrEnable = value;
                if (serialPort != null)
                {
                    serialPort.DtrEnable = dtrEnable;
                }
            }
        }

        private bool rtsEnable = false;
        public bool RtsEnable
        {
            get { return rtsEnable; }
            set
            {
                rtsEnable = value;
                if (serialPort != null)
                {
                    serialPort.RtsEnable = rtsEnable;
                }
            }
        }

        public ModbusRTU()
        {
            this.serialPort = new SerialPort();
        }

        //0X01-0
        //一行12个
        private static readonly byte[] aucCRCHi = {
            0x00,0xC1,0x81,0x40,0x01,0xC0,0x80,0x41,0x01,0xC0,0x80,0x41,0x00,0xC1,0x81,0x40,
            0x01,0xC0,0x80,0x41,0x00,0xC1,0x81,0x40,0x00,0xC1,0x81,0x40,0x01,0xC0,0x80,0x41,
            0x01,0xC0,0x80,0x41,0x00,0xC1,0x81,0x40,0x00,0xC1,0x81,0x40,0x01,0xC0,0x80,0x41,
            0x00,0xC1,0x81,0x40,0x01,0xC0,0x80,0x41,0x01,0xC0,0x80,0x41,0x00,0xC1,0x81,0x40,
            0x01,0xC0,0x80,0x41,0x00,0xC1,0x81,0x40,0x00,0xC1,0x81,0x40,0x01,0xC0,0x80,0x41,
            0x00,0xC1,0x81,0x40,0x01,0xC0,0x80,0x41,0x01,0xC0,0x80,0x41,0x00,0xC1,0x81,0x40,
            0x00,0xC1,0x81,0x40,0x01,0xC0,0x80,0x41,0x01,0xC0,0x80,0x41,0x00,0xC1,0x81,0x40,
            0x01,0xC0,0x80,0x41,0x00,0xC1,0x81,0x40,0x00,0xC1,0x81,0x40,0x01,0xC0,0x80,0x41,
            0x01,0xC0,0x80,0x41,0x00,0xC1,0x81,0x40,0x00,0xC1,0x81,0x40,0x01,0xC0,0x80,0x41,
            0x00,0xC1,0x81,0x40,0x01,0xC0,0x80,0x41,0x01,0xC0,0x80,0x41,0x00,0xC1,0x81,0x40,
            0x00,0xC1,0x81,0x40,0x01,0xC0,0x80,0x41,0x01,0xC0,0x80,0x41,0x00,0xC1,0x81,0x40,
            0x01,0xC0,0x80,0x41,0x00,0xC1,0x81,0x40,0x00,0xC1,0x81,0x40,0x01,0xC0,0x80,0x41,
            0x00,0xC1,0x81,0x40,0x01,0xC0,0x80,0x41,0x01,0xC0,0x80,0x41,0x00,0xC1,0x81,0x40,
            0x01,0xC0,0x80,0x41,0x00,0xC1,0x81,0x40,0x00,0xC1,0x81,0x40,0x01,0xC0,0x80,0x41,
            0x01,0xC0,0x80,0x41,0x00,0xC1,0x81,0x40,0x00,0xC1,0x81,0x40,0x01,0xC0,0x80,0x41,
            0x00,0xC1,0x81,0x40,0x01,0xC0,0x80,0x41,0x01,0xC0,0x80,0x41,0x00,0xC1,0x81,0x40
        };


        private static readonly byte[] aucCRCLo = {
            0x00,0xC0,0xC1,0x01,0xC3,0x03,0x02,0xC2,0xC6,0x06,0x07,0xC7,0x05,0xC5,0xC4,0x04,
            0xCC,0x0C,0x0D,0xCD,0x0F,0xCF,0xCE,0x0E,0x0A,0xCA,0xCB,0x0B,0xC9,0x09,0x08,0xC8,
            0xD8,0x18,0x19,0xD9,0x1B,0xDB,0xDA,0x1A,0x1E,0xDE,0xDF,0x1F,0xDD,0x1D,0x1C,0xDC,
            0x14,0xD4,0xD5,0x15,0xD7,0x17,0x16,0xD6,0xD2,0x12,0x13,0xD3,0x11,0xD1,0xD0,0x10,
            0xF0,0x30,0x31,0xF1,0x33,0xF3,0xF2,0x32,0x36,0xF6,0xF7,0x37,0xF5,0x35,0x34,0xF4,
            0x3C,0xFC,0xFD,0x3D,0xFF,0x3F,0x3E,0xFE,0xFA,0x3A,0x3B,0xFB,0x39,0xF9,0xF8,0x38,
            0x28,0xE8,0xE9,0x29,0xEB,0x2B,0x2A,0xEA,0xEE,0x2E,0x2F,0xEF,0x2D,0xED,0xEC,0x2C,
            0xE4,0x24,0x25,0xE5,0x27,0xE7,0xE6,0x26,0x22,0xE2,0xE3,0x23,0xE1,0x21,0x20,0xE0,
            0xA0,0x60,0x61,0xA1,0x63,0xA3,0xA2,0x62,0x66,0xA6,0xA7,0x67,0xA5,0x65,0x64,0xA4,
            0x6C,0xAC,0xAD,0x6D,0xAF,0x6F,0x6E,0xAE,0xAA,0x6A,0x6B,0xAB,0x69,0xA9,0xA8,0x68,
            0x78,0xB8,0xB9,0x79,0xBB,0x7B,0x7A,0xBA,0xBE,0x7E,0x7F,0xBF,0x7D,0xBD,0xBC,0x7C,
            0xB4,0x74,0x75,0xB5,0x77,0xB7,0xB6,0x76,0x72,0xB2,0xB3,0x73,0xB1,0x71,0x70,0xB0,
            0x50,0x90,0x91,0x51,0x93,0x53,0x52,0x92,0x96,0x56,0x57,0x97,0x55,0x95,0x94,0x54,
            0x9C,0x5C,0x5D,0x9D,0x5F,0x9F,0x9E,0x5E,0x5A,0x9A,0x9B,0x5B,0x99,0x59,0x58,0x98,
            0x88,0x48,0x49,0x89,0x4B,0x8B,0x8A,0x4A,0x4E,0x8E,0x8F,0x4F,0x8D,0x4D,0x4C,0x8C,
            0x44,0x84,0x85,0x45,0x87,0x47,0x46,0x86,0x82,0x42,0x43,0x83,0x41,0x81,0x80,0x40
        };


        private byte[] Crc16(byte[] pucFrame, int usLen)
        {
            int i = 0;
            byte[] res = new byte[2] { 0xFF, 0xFF };
            ushort iIndex;
            while (usLen-- > 0)
            {
                iIndex = (ushort)(res[0] ^ pucFrame[i++]);
                res[0] = (byte)(res[1] ^ aucCRCHi[iIndex]);
                res[1] = aucCRCLo[iIndex];
            }
            return res;
        }

        private bool checkCrc(byte[] value)
        {
            if (value == null) return false;
            if (value.Length <= 2) return false;
            int length = value.Length;
            byte[] buf = new byte[length - 2];
            Array.Copy(value, 0, buf, 0, buf.Length);
            byte[] crc = Crc16(buf, buf.Length);
            if (crc[0] == value[length - 2] && crc[1] == value[length - 1]) return true;
            return false;
        }


        ///数组比较方法
        /// <param name="a">数组a</param>
        /// <param name="b">数组b</param>
        /// <returns>是否相等</returns>
        private bool ArrayCompare(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i]) return false;
            }
            return true;
        }

        ///布尔数组转换为字节数组
        /// <param name="value">布尔数组</param>
        /// <returns>字节数组</returns>
        private byte[] GetByteArrayFromBoolArray(bool[] value)
        {
            int byteArrayLength = value.Length % 8 == 0 ? value.Length / 8 : value.Length / 8 + 1;
            byte[] result = new byte[byteArrayLength];
            for (int i = 0; i < result.Length; i++)
            {
                //获得每个字节的值
                int total = value.Length < 8 * (i + 1) ? value.Length - 8 * i : 8;
                for (int j = 0; j < total; j++)
                {
                    if (value[i * 8 + j])
                    {
                        result[i] |= (byte)(1 << j);
                    }
                }

            }
            return result;
        }

        #region 连接和断开
        // <summary>
        //串口名，波特率，校验位，数据位，停止位
        //</summary>
        public bool Connect(string portName, int butaRate, Parity parity, int dataBits = 0, StopBits stopBits = StopBits.One)
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.Close();
            }
            serialPort.PortName = portName;
            serialPort.BaudRate = butaRate;
            serialPort.Parity = parity;
            serialPort.DataBits = dataBits;
            serialPort.StopBits = stopBits;

            serialPort.ReadTimeout = ReadTimeout;
            serialPort.WriteTimeout = WriteTimeout;
            try
            {
                serialPort.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show("串口打开失败：" + ex.Message);
                return false;
            }
            return true;
        }

        public void Disconnect()
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.Close();

            }
        }
        #endregion


        private bool SendAndRecive(byte[] send, ref byte[] receive)
        {
            try
            {
                this.serialPort.Write(send, 0, send.Length);
                //定义接收缓冲区
                byte[] buffer = new byte[1024];
                MemoryStream stream = new MemoryStream();
                //开始时间
                DateTime start = DateTime.Now;
                //如果大于0ms，读出来放内存
                while (true)
                {
                    Thread.Sleep(200);
                    if (serialPort.BytesToRead > 0)
                    {
                        int count = this.serialPort.Read(buffer, 0, buffer.Length);
                        stream.Write(buffer, 0, count);
                    }
                    //如果没有数据，等待
                    else
                    {
                        //如果内存有气体，跳出循环
                        if (stream.Length > 0)
                        {
                            break;
                        }
                        //如果超时，返回false
                        else if ((DateTime.Now - start).TotalMilliseconds > ReceiveTimeout)
                        {
                            return false;
                        }
                    }
                }
                receive = stream.ToArray();
                return true;
            }
            catch (Exception)
            {
                return false;
            }


        }


        #region 功能
        //01h 读取输出线圈
        // <param name="slaveId">从站地址</param>
        // <param name="startAddress">起始地址</param>
        // <param name="numberOfPoints">读取长度</param>
        public byte[] ReadOutPutCoil(byte slaveId, ushort startAddress, ushort length)
        {
            //拼接报文
            List<byte> SendCommend = new List<byte>();
            //从站地址+功能码+起始地址+线圈数量+Crc16
            SendCommend.Add(slaveId);
            //功能码
            SendCommend.Add(0x01);
            //起始地址 高位和低位
            SendCommend.Add((byte)(startAddress / 256));
            SendCommend.Add((byte)(startAddress % 256));

            //线圈数量
            SendCommend.Add((byte)(length / 256));
            SendCommend.Add((byte)(length % 256));
            //Crc
            byte[] crc = Crc16(SendCommend.ToArray(), SendCommend.Count);
            SendCommend.AddRange(crc);
            //发送报文
            //接收报文
            byte[] receive = null;
            int byteLength = length % 8 == 0 ? length / 8 : length / 8 + 1;
            if (SendAndRecive(SendCommend.ToArray(), ref receive))
            {
                // 功能码1字节 起始地址2字节 crc2字节+数据字节
                if (checkCrc(receive) && receive.Length == 5 + byteLength)
                {
                    if (receive[1] == 0x01 && receive[0] == slaveId)
                    {
                        byte[] result = new byte[byteLength];
                        Array.Copy(receive, 3, result, 0, byteLength);
                        return result;
                    }
                }
            }
            return null;
        }


        //02h读取输入线圈
        public byte[] ReadInputCoil(byte slaveId, ushort startAddress, ushort length)
        {
            //拼接报文
            List<byte> SendCommend = new List<byte>();
            //从站地址+功能码+起始地址+线圈数量+Crc16
            SendCommend.Add(slaveId);
            //功能码
            SendCommend.Add(0x02);
            //起始地址 高位和低位
            SendCommend.Add((byte)(startAddress / 256));
            SendCommend.Add((byte)(startAddress % 256));

            //线圈数量
            SendCommend.Add((byte)(length / 256));
            SendCommend.Add((byte)(length % 256));
            //Crc
            byte[] crc = Crc16(SendCommend.ToArray(), SendCommend.Count);
            SendCommend.AddRange(crc);
            //发送报文
            //接收报文
            byte[] receive = null;
            int byteLength = length % 8 == 0 ? length / 8 : length / 8 + 1;
            if (SendAndRecive(SendCommend.ToArray(), ref receive))
            {
                // 功能码1字节 起始地址2字节 crc2字节+数据字节
                if (checkCrc(receive) && receive.Length == 5 + byteLength)
                {
                    if (receive[1] == 0x02 && receive[0] == slaveId)
                    {
                        byte[] result = new byte[byteLength];
                        Array.Copy(receive, 3, result, 0, byteLength);
                        return result;
                    }
                }
            }
            //验证报文
            //
            return null;
        }

        /// 03h读取4区输出寄存器
        /// </summary>
        /// <param name="slaveId">从站地址</param>
        /// <param name="startAddress">起始地址</param>
        /// <param name="length">读取数量</param>
        /// <returns>读取到的寄存器值</returns>
        public byte[] ReadOutPutRegister(byte slaveId, ushort startAddress, ushort length)
        {
            //拼接报文
            List<byte> SendCommend = new List<byte>();
            SendCommend.Add(slaveId);
            SendCommend.Add(0x03);
            SendCommend.Add((byte)(startAddress / 256));
            SendCommend.Add((byte)(startAddress % 256));
            //寄存器数量
            SendCommend.Add((byte)(length / 256));
            SendCommend.Add((byte)(length % 256));
            //crc
            SendCommend.AddRange(Crc16(SendCommend.ToArray(), SendCommend.Count));
            byte[] rec = null;
            int byteLength = length * 2;
            if (SendAndRecive(SendCommend.ToArray(), ref rec))
            {
                if (checkCrc(rec) && rec.Length == 5 + byteLength)
                {
                    if (rec[1] == 0x03 && rec[0] == slaveId)
                    {
                        byte[] result = new byte[byteLength];
                        Array.Copy(rec, 3, result, 0, byteLength);
                        return result;
                    }
                }
            }
            return null;
        }


        /// <summary>
        /// 04读取输入寄存器
        /// </summary>
        /// <param name="slaveId">从站地址</param>
        /// <param name="startAddress">起始地址</param>
        /// <param name="length">读取数量</param>
        /// <returns>读取到的寄存器值</returns>
        public byte[] ReadInputRegister(byte slaveId, ushort startAddress, ushort length)
        {
            //拼接报文
            List<byte> SendCommend = new List<byte>();
            SendCommend.Add(slaveId);
            SendCommend.Add(0x04);
            SendCommend.Add((byte)(startAddress / 256));
            SendCommend.Add((byte)(startAddress % 256));
            //寄存器数量
            SendCommend.Add((byte)(length / 256));
            SendCommend.Add((byte)(length % 256));
            //crc
            SendCommend.AddRange(Crc16(SendCommend.ToArray(), SendCommend.Count));
            byte[] rec = null;
            int byteLength = length * 2;
            
            if (SendAndRecive(SendCommend.ToArray(), ref rec))
            {
                if (checkCrc(rec) && rec.Length == 5 + byteLength)
                {
                    if (rec[1] == 0x04 && rec[0] == slaveId)
                    {
                        byte[] result = new byte[byteLength];
                        Array.Copy(rec, 3, result, 0, byteLength);
                        
                        return result;
                    }
                }
            }
            
            return null;
        }





        //预置单线圈05h
        public bool PreSetSingleCoil(byte slaveId, ushort startAddress, bool value)
        {
            List<byte> SendCommend = new List<byte>();
            SendCommend.Add(slaveId);
            SendCommend.Add(0x05);
            SendCommend.Add((byte)(startAddress / 256));
            SendCommend.Add((byte)(startAddress % 256));
            SendCommend.Add((byte)(value ? 0xff : 0x00));
            SendCommend.Add(0x00);
            SendCommend.AddRange(Crc16(SendCommend.ToArray(), SendCommend.Count));

            //发送报文
            //接收报文
            byte[] receive = null;
            if (SendAndRecive(SendCommend.ToArray(), ref receive))
            {
                if (checkCrc(receive) && receive.Length == 8)
                {
                    //比较rec和send是否相等
                    return ArrayCompare(receive, SendCommend.ToArray());
                }
            }
            return false;
        }


        ///预置单寄存器06h
        /// <param name="slaveId">从站地址</param>
        /// <param name="startAddress">起始地址</param>
        /// <param name="value">预置值</param>
        /// <returns>是否成功</returns>
        public bool PreSetSingleRegister(byte slaveId, ushort startAddress, byte[] value)
        {
            List<byte> SendCommend = new List<byte>();
            SendCommend.Add(slaveId);
            SendCommend.Add(0x06);
            SendCommend.Add((byte)(startAddress / 256));
            SendCommend.Add((byte)(startAddress % 256));
            SendCommend.AddRange(value);
            SendCommend.AddRange(Crc16(SendCommend.ToArray(), SendCommend.Count));
            //发送报文
            //接收报文
            byte[] receive = null;
            if (SendAndRecive(SendCommend.ToArray(), ref receive))
            {
                if (checkCrc(receive) && receive.Length == 8)
                {
                    //比较rec和send是否相等
                    return ArrayCompare(receive, SendCommend.ToArray());
                }
            }
            return false;
        }

        public bool PreSetSingleCoil(byte slaveId, ushort startAddress, short value)
        {
            return PreSetSingleRegister(slaveId, startAddress, BitConverter.GetBytes(value).Reverse().ToArray());
        }

        public bool PreSetSingleRegister(byte slaveId, ushort startAddress, ushort value)
        {
            return PreSetSingleRegister(slaveId, startAddress, BitConverter.GetBytes(value).Reverse().ToArray());
        }

        public bool PreSetSingleRegister(byte slaveId, ushort startAddress, short value)
        {
            return PreSetSingleRegister(slaveId, startAddress, BitConverter.GetBytes(value).Reverse().ToArray());
        }


        //线圈1个字节  寄存器2个字节
        //10h预置多寄存器  将x号站点从startAddress开始的registerLength个寄存器预置为values
        public bool PreSetMultipleRegisters(byte slaveId, ushort startAddress, byte[] values)
        {

            if (values == null || values.Length % 2 != 0 || values.Length == 0)
            {
                return false;
            }
            //拼接报文 从站地址 功能码 起始寄存器地址 寄存器数量 字节计数 数据 crc
            List<byte> SendCommend = new List<byte>();
            int registerLength = values.Length / 2;
            SendCommend.Add(slaveId);
            SendCommend.Add(0x10);

            //起始寄存器地址
            SendCommend.Add((byte)(startAddress / 256));
            SendCommend.Add((byte)(startAddress % 256));
            //寄存器数量 两个字节
            SendCommend.Add((byte)(registerLength / 256));
            SendCommend.Add((byte)(registerLength % 256));
            //字节计数 一个字节
            SendCommend.Add((byte)(values.Length));
            //数据
            SendCommend.AddRange(values);
            SendCommend.AddRange(Crc16(SendCommend.ToArray(), SendCommend.Count));

            byte[] receive = null;
            if (SendAndRecive(SendCommend.ToArray(), ref receive))
            {
                //验证前六个字节是否相等
                for (int i = 0; i < 6; i++)
                {
                    if (SendCommend[i] != receive[i])
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }



        //0fh预置多线圈
        public bool PreSetMultipleCoil(byte slaveId, ushort startAddress, bool[] value)
        {
            //从站地址 功能码 起始地址 线圈数量 字节计数 数据 crc
            List<byte> SendCommend = new List<byte>();
            //将bool数组转换为byte数组
            byte[] setArray = this.GetByteArrayFromBoolArray(value);
            SendCommend.Add(slaveId);
            SendCommend.Add(0x0f);
            SendCommend.Add((byte)(startAddress / 256));
            SendCommend.Add((byte)(startAddress % 256));
            //线圈数量
            SendCommend.Add((byte)(value.Length / 256));
            SendCommend.Add((byte)(value.Length % 256));
            //字节计数
            SendCommend.Add((byte)(setArray.Length));
            //数据
            SendCommend.AddRange(setArray);
            //crc
            SendCommend.AddRange(Crc16(SendCommend.ToArray(), SendCommend.Count));
            //发送报文
            //接收报文
            byte[] receive = null;
            if (SendAndRecive(SendCommend.ToArray(), ref receive))
            {
                if (checkCrc(receive) && receive.Length == 8)
                {
                    //验证发送和接收前六个字节是否相等
                    for (int i = 0; i < 6; i++)
                    {
                        if (SendCommend[i] != receive[i])
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }
            return false;
        }
        #endregion

    }
}