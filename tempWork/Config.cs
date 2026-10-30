using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;

namespace tempWork
{
    public class Config
    {
        //96n81
        public string portName {get;set;}
        public int baudRate {get;set;}
        public Parity Parity {get;set;}
        public int DataBits {get;set;}
        public StopBits StopBits {get;set;}

        /// <summary>
        /// 从站ID
        /// </summary>
        public byte slaveId1 {get;set;}
        public byte slaveId2 {get;set;}
        public byte slaveId3 {get;set;}
        public byte slaveId4 {get;set;}
       
    }
}
