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
        /// <param name="slaveId">从站id</param>
        /// <param name="startAddress">查询开始地址</param>
        /// <param name="length">查询个数</param>
        /// <returns>地址和布尔值的映射</returns>
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

        /// <summary>
        /// 读保持寄存器（可读写寄存器）
        /// </summary>
        /// <param name="slaveId">从站id</param>
        /// <param name="startAddress">查询开始地址</param>
        /// <param name="length">查询个数</param>
        /// <returns>地址和寄存器值的映射</returns>
        protected Dictionary<ushort, ushort> ReadWritableRegister(byte slaveId, ushort startAddress, ushort dataLengh)
        {
            try
            {
                MemoryStream memoryStream = new MemoryStream();
                byte[] addressBytes = NumberBaseConvertor.SplitUshort2Byte(startAddress);
                byte[] lengthBytes = NumberBaseConvertor.SplitUshort2Byte(dataLengh);


                // 获取两个byte作为事务id
                byte[] transactionBytes = TransactionIdGenerator.getTransactionIdBytesByIp(ip, port);
                byte[] requestMbapHead = new byte[7] { transactionBytes[0], transactionBytes[1], 0x00, 0x00, 0x00, 0x06, slaveId };
                byte[] requestBody = new byte[5] { ModbusConstans.ModbusFunctionCodeReadHoldingRegister, addressBytes[0], addressBytes[1], lengthBytes[0], lengthBytes[1] };

                memoryStream.Write(requestMbapHead, 0, requestMbapHead.Length);
                memoryStream.Write(requestBody, 0, requestBody.Length);
                byte[] requestBytes = memoryStream.ToArray();
                // 发起请求，拿到响应报文
                byte[] responseBytes = ModbusTCPClientManager.modbusRequestByBytes(ip, port, requestBytes);

                // 解析响应报文

                // 校验transactionId
                if (transactionBytes[0] != responseBytes[0] || transactionBytes[1] != responseBytes[1])
                {
                    // TODO warning 事务id对不上 不处理但是需要记录异常日志
                }

                // 获取响应数据字节数
                int responseDataLengh = responseBytes[8];
                byte[] dataBytes = responseBytes.Skip(9).Take(responseDataLengh).ToArray();

                Dictionary<ushort, ushort> resultKV = new Dictionary<ushort, ushort>();
                for (int i = 0; i < dataLengh; i++)
                {
                    resultKV.Add(startAddress++, dataBytes[i]);
                }

                return resultKV;

            }
            catch (Exception e)
            {
                // TODO 异常
                return new Dictionary<ushort, ushort>();
            }
        }

        /// <summary>
        /// 写单个线圈
        /// </summary>
        /// <param name="slaveId"></param>
        /// <param name="writeAddress"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        protected bool WriteSingleCoil(byte slaveId, ushort writeAddress, bool input)
        {
            try
            {
                byte[] addressBytes = NumberBaseConvertor.SplitUshort2Byte(writeAddress);

                byte[] transactionBytes = TransactionIdGenerator.getTransactionIdBytesByIp(ip, port);
                byte[] requestMbapHead = new byte[7] { transactionBytes[0], transactionBytes[1], 0x00, 0x00, 0x00, 0x06, slaveId };
                byte[] requestBody = new byte[5] { ModbusConstans.ModbusFunctionCodeWriteSingleCoil, addressBytes[0], addressBytes[1], 0xFF, 0x00 };

                MemoryStream memoryStream = new MemoryStream();
                memoryStream.Write(requestMbapHead, 0, requestMbapHead.Length);
                memoryStream.Write(requestBody, 0, requestBody.Length);

                byte[] requestBytes = memoryStream.ToArray();
                // 发起请求，拿到响应报文
                byte[] responseBytes = ModbusTCPClientManager.modbusRequestByBytes(ip, port, requestBytes);

                // 校验请求与返回报文是否一致
                if (!requestBytes.SequenceEqual(responseBytes))
                {
                    // TODO 异常 操作成功但是要记录错误日志
                }

                return false;
            }
            catch (Exception e)
            {
                // 异常
                return false;
            }
        }

        /// <summary>
        /// 写多个线圈（用字节作为写入结果）
        /// </summary>
        /// <param name="slaveId"></param>
        /// <param name="writeStartAddress"></param>
        /// <param name="dataLengh"></param>
        /// <param name="writeData"></param>
        /// <returns></returns>
        protected bool WriteMultiCoil(byte slaveId, ushort writeStartAddress, byte[] writeData)
        {
            try
            {
                // 校验
                if (writeData.Length + 7 > byte.MaxValue)
                {
                    throw new ArgumentException("写入多个线圈数据时，数据超长，最多同时写入248个字节");
                }

                MemoryStream memoryStream = new MemoryStream();
                byte[] addressBytes = NumberBaseConvertor.SplitUshort2Byte(writeStartAddress);
                byte[] lengthBytes = NumberBaseConvertor.SplitUshort2Byte((byte)(writeData.Length));


                // 获取两个byte作为事务id
                byte[] transactionBytes = TransactionIdGenerator.getTransactionIdBytesByIp(ip, port);
                byte[] requestMbapHead = new byte[7] { transactionBytes[0], transactionBytes[1], 0x00, 0x00, 0x00, (byte)(7 + writeData.Length), slaveId };

                // 拼接请求报文
                MemoryStream requestMemoryStream = new MemoryStream();
                requestMemoryStream.Write(requestMbapHead, 0, requestMbapHead.Length);
                requestMemoryStream.Write(new byte[6] { ModbusConstans.ModbusFunctionCodeWriteMultipleCoils, addressBytes[0], addressBytes[1], lengthBytes[0], lengthBytes[1], (byte)writeData.Length }, 0, 6);
                requestMemoryStream.Write(writeData, 0, writeData.Length);
                byte[] requestBytes = requestMemoryStream.ToArray();

                // 发起请求，拿到响应报文
                byte[] responseBytes = ModbusTCPClientManager.modbusRequestByBytes(ip, port, requestBytes);

                // 解析响应报文
                if (!requestBytes.SequenceEqual(responseBytes))
                {
                    // 请求报文与响应报文不一致，不处理但记录异常日志


                }

                return true;


            }
            catch (Exception e)
            {
                // TODO 异常
                return false;
            }
        }

        /// <summary>
        /// 写单个保持寄存器
        /// </summary>
        /// <param name="slaveId"></param>
        /// <param name="writeAddress"></param>
        /// <param name="writeData"></param>
        /// <returns></returns>
        protected bool WriteSingleRegister(byte slaveId, ushort writeAddress, ushort writeData)
        {
            try
            {
                byte[] addressBytes = NumberBaseConvertor.SplitUshort2Byte(writeAddress);
                byte[] writeDataBytes = NumberBaseConvertor.SplitUshort2Byte(writeData);

                byte[] transactionBytes = TransactionIdGenerator.getTransactionIdBytesByIp(ip, port);
                byte[] requestMbapHead = new byte[7] { transactionBytes[0], transactionBytes[1], 0x00, 0x00, 0x00, 0x06, slaveId };
                byte[] requestBody = new byte[5] { ModbusConstans.ModbusFunctionCodeWriteSingleRegister, addressBytes[0], addressBytes[1], writeDataBytes[0], writeDataBytes[1] };

                MemoryStream memoryStream = new MemoryStream();
                memoryStream.Write(requestMbapHead, 0, requestMbapHead.Length);
                memoryStream.Write(requestBody, 0, requestBody.Length);

                byte[] requestBytes = memoryStream.ToArray();
                // 发起请求，拿到响应报文
                byte[] responseBytes = ModbusTCPClientManager.modbusRequestByBytes(ip, port, requestBytes);

                // 校验请求与返回报文是否一致
                if (!requestBytes.SequenceEqual(responseBytes))
                {
                    // TODO 异常 操作成功但是要记录错误日志
                }

                return false;
            }
            catch (Exception e)
            {
                // 异常
                return false;
            }

        }

        /// <summary>
        ///  写多个保持寄存器
        /// </summary>
        /// <param name="slaveId"></param>
        /// <param name="writeStartAddress"></param>
        /// <param name="dataLengh"></param>
        /// <param name="writeData"></param>
        /// <returns></returns>
        protected bool WriteMultiRegister(byte slaveId, ushort writeStartAddress, byte[] writeData)
        {
            try
            {
                // 校验
                if (writeData.Length + 7 > byte.MaxValue)
                {
                    throw new ArgumentException("写入多个寄存器数据时，数据超长，最多同时写入248个字节");
                }

                MemoryStream memoryStream = new MemoryStream();
                byte[] addressBytes = NumberBaseConvertor.SplitUshort2Byte(writeStartAddress);
                byte[] lengthBytes = NumberBaseConvertor.SplitUshort2Byte((byte)(writeData.Length));


                // 获取两个byte作为事务id
                byte[] transactionBytes = TransactionIdGenerator.getTransactionIdBytesByIp(ip, port);
                byte[] requestMbapHead = new byte[7] { transactionBytes[0], transactionBytes[1], 0x00, 0x00, 0x00, (byte)(7 + writeData.Length), slaveId };

                // 拼接请求报文
                MemoryStream requestMemoryStream = new MemoryStream();
                requestMemoryStream.Write(requestMbapHead, 0, requestMbapHead.Length);
                requestMemoryStream.Write(new byte[6] { ModbusConstans.ModbusFunctionCodeWriteMultipleRegisters, addressBytes[0], addressBytes[1], lengthBytes[0], lengthBytes[1], (byte)writeData.Length }, 0, 6);
                requestMemoryStream.Write(writeData, 0, writeData.Length);
                byte[] requestBytes = requestMemoryStream.ToArray();

                // 发起请求，拿到响应报文
                byte[] responseBytes = ModbusTCPClientManager.modbusRequestByBytes(ip, port, requestBytes);

                // 解析响应报文
                if (!requestBytes.SequenceEqual(responseBytes))
                {
                    // 请求报文与响应报文不一致，不处理但记录异常日志


                }

                return true;


            }
            catch (Exception e)
            {
                // TODO 异常
                return false;
            }
        }


        /// <summary>
        /// 读只读线圈（离散输入）
        /// </summary>
        /// <param name="slaveId"></param>
        /// <param name="startAddress"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        protected Dictionary<ushort, bool> ReadReadOnlyCoil(byte slaveId, ushort startAddress, ushort length)
        {
            try
            {
                MemoryStream memoryStream = new MemoryStream();
                byte[] addressBytes = NumberBaseConvertor.SplitUshort2Byte(startAddress);
                byte[] lengthBytes = NumberBaseConvertor.SplitUshort2Byte(length);


                // 获取两个byte作为事务id
                byte[] transactionBytes = TransactionIdGenerator.getTransactionIdBytesByIp(ip, port);
                byte[] requestMbapHead = new byte[7] { transactionBytes[0], transactionBytes[1], 0x00, 0x00, 0x00, 0x06, slaveId };
                byte[] requestBody = new byte[5] { ModbusConstans.ModbusFunctionCodeReadDiscreteInputs, addressBytes[0], addressBytes[1], lengthBytes[0], lengthBytes[1] };

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

        /// <summary>
        /// 读只读寄存器（输入寄存器）
        /// </summary>
        /// <param name="slaveId"></param>
        /// <param name="startAddress"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        protected Dictionary<ushort, ushort> ReadReadOnlyRegister(byte slaveId, ushort startAddress, ushort dataLengh)
        {
            try
            {
                MemoryStream memoryStream = new MemoryStream();
                byte[] addressBytes = NumberBaseConvertor.SplitUshort2Byte(startAddress);
                byte[] lengthBytes = NumberBaseConvertor.SplitUshort2Byte(dataLengh);


                // 获取两个byte作为事务id
                byte[] transactionBytes = TransactionIdGenerator.getTransactionIdBytesByIp(ip, port);
                byte[] requestMbapHead = new byte[7] { transactionBytes[0], transactionBytes[1], 0x00, 0x00, 0x00, 0x06, slaveId };
                byte[] requestBody = new byte[5] { ModbusConstans.ModbusFunctionCodeReadHoldingRegister, addressBytes[0], addressBytes[1], lengthBytes[0], lengthBytes[1] };

                memoryStream.Write(requestMbapHead, 0, requestMbapHead.Length);
                memoryStream.Write(requestBody, 0, requestBody.Length);
                byte[] requestBytes = memoryStream.ToArray();
                // 发起请求，拿到响应报文
                byte[] responseBytes = ModbusTCPClientManager.modbusRequestByBytes(ip, port, requestBytes);

                // 解析响应报文

                // 校验transactionId
                if (transactionBytes[0] != responseBytes[0] || transactionBytes[1] != responseBytes[1])
                {
                    // TODO warning 事务id对不上 不处理但是需要记录异常日志
        }

                // 获取响应数据字节数
                int responseDataLengh = responseBytes[8];
                byte[] dataBytes = responseBytes.Skip(9).Take(responseDataLengh).ToArray();

                Dictionary<ushort, ushort> resultKV = new Dictionary<ushort, ushort>();
                for (int i = 0; i < dataLengh; i++)
                {
                    resultKV.Add(startAddress++, dataBytes[i]);
                }

                return resultKV;

            }
            catch (Exception e)
            {
                // TODO 异常
                return new Dictionary<ushort, ushort>();
            }
        }



    }
}
