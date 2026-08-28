using moju.constants;
using moju.tool;
using muju.modbus;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace moju.device.interfaces
{
    internal class ModbusSlave
    {

        String ip { get; set; }
        int port { get; set; }

        /// <summary>
        /// 把ip和端口组合成ip:port的格式
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        private static String CombineAddress(String ip, long port)
        {
            return ip + ":" + port.ToString();
        }

        public ModbusSlave(String ip, int port)
        {
            this.ip = ip;
            this.port = port;
        }

        /// <summary>
        /// 读可读写线圈
        /// </summary>
        /// <param name="slaveId"></param>
        /// <param name="startAddress"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public Dictionary<ushort, bool> ReadWritableCoil(byte slaveId, ushort startAddress, ushort length)
        {
            try
            {
                MemoryStream memoryStream = new MemoryStream();
                byte[] addressBytes = NumberBaseConvertor.SplitUshort2Byte(startAddress);
                byte[] lengthBytes = NumberBaseConvertor.SplitUshort2Byte(length);


                // 获取两个byte作为事务id
                byte[] transactionBytes = TransactionIdGenerator.getTransactionIdBytesByIp(ip, port);
                byte[] requestMbapHead = new byte[7] { transactionBytes[0], transactionBytes[1], 0x00, 0x00, 0x00, 0x06, slaveId };
                byte[] requestBody = new byte[5] { ModbusConstans.ModbusFunctionCodeReadCoil, addressBytes[0], addressBytes[1], lengthBytes[0], lengthBytes[1] };

                memoryStream.Write(requestMbapHead, 0, requestMbapHead.Length);
                memoryStream.Write(requestBody, 0, requestBody.Length);
                byte[] requestBytes = memoryStream.ToArray();
                // 发起请求，拿到响应报文
                byte[] responseBytes = ModbusTCPClientManager.modbusRequestByBytes(ip, port, requestBytes);

                // 解析响应报文

                // 校验transactionId
                if (transactionBytes[0] != responseBytes[0] || transactionBytes[1] != responseBytes[1])
                {
                    // TODO warning 事务id对不上
                }

                // 获取响应数据字节数
                int responseDataLengh = responseBytes[8];
                byte[] dataBytes = responseBytes.Skip(9).Take(responseDataLengh).ToArray();

                Dictionary<ushort, bool> resultKV = new Dictionary<ushort, bool>();

                BitArray bitArray = new BitArray(dataBytes);
                for (int i = 0; i < length; i++)
                {
                    resultKV.Add(startAddress++, bitArray[i]);
                }
                return resultKV;

            }
            catch (Exception e)
            {
                // TODO 异常
                return new Dictionary<ushort, bool>();
            }


        }

        ///// <summary>
        ///// 读保持寄存器（可读写寄存器）
        ///// </summary>
        ///// <param name="slaveId"></param>
        ///// <param name="startAddress"></param>
        ///// <param name="length"></param>
        ///// <returns></returns>
        //protected byte[] ReadWritableRegister(byte slaveId, ushort startAddress, ushort length)
        //{
        //
        //}
        //
        ///// <summary>
        ///// 写单个线圈
        ///// </summary>
        ///// <param name="slaveId"></param>
        ///// <param name="writeAddress"></param>
        ///// <param name="input"></param>
        ///// <returns></returns>
        //protected byte[] WriteSingleCoil(byte slaveId, ushort writeAddress, bool input)
        //{
        //
        //}
        //
        ///// <summary>
        ///// 写多个线圈
        ///// </summary>
        ///// <param name="slaveId"></param>
        ///// <param name="writeStartAddress"></param>
        ///// <param name="dataLengh"></param>
        ///// <param name="writeData"></param>
        ///// <returns></returns>
        //protected byte[] WriteMultiCoil(byte slaveId, ushort writeStartAddress, byte dataLengh, byte[] writeData)
        //{
        //
        //}
        //
        ///// <summary>
        ///// 写单个保持寄存器
        ///// </summary>
        ///// <param name="slaveId"></param>
        ///// <param name="writeAddress"></param>
        ///// <param name="writeData"></param>
        ///// <returns></returns>
        //protected byte[] WriteSingleRegister(byte slaveId, ushort writeAddress, ushort writeData)
        //{
        //
        //}
        //
        ///// <summary>
        /////  写多个保持寄存器
        ///// </summary>
        ///// <param name="slaveId"></param>
        ///// <param name="writeStartAddress"></param>
        ///// <param name="dataLengh"></param>
        ///// <param name="writeData"></param>
        ///// <returns></returns>
        //protected byte[] WriteMultiRegister(byte slaveId, ushort writeStartAddress, byte dataLengh, byte[] writeData)
        //{
        //
        //}
        //
        //
        ///// <summary>
        ///// 读只读线圈（离散输入）
        ///// </summary>
        ///// <param name="slaveId"></param>
        ///// <param name="startAddress"></param>
        ///// <param name="length"></param>
        ///// <returns></returns>
        //protected byte[] ReadReadOnlyCoil(byte slaveId, ushort startAddress, ushort length)
        //{
        //
        //}
        //
        ///// <summary>
        ///// 读只读寄存器（输入寄存器）
        ///// </summary>
        ///// <param name="slaveId"></param>
        ///// <param name="startAddress"></param>
        ///// <param name="length"></param>
        ///// <returns></returns>
        //protected byte[] ReadReadOnlyRegister(byte slaveId, ushort startAddress, ushort length)
        //{
        //
        //}



    }
}
