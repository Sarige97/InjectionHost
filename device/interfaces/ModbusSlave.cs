using moju.constants;
using moju.domain;
using moju.log;
using moju.modbus;
using moju.tool;
using muju.modbus;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static moju.device.interfaces.ModbusSlave;

namespace moju.device.interfaces
{
    internal abstract class ModbusSlave
    {

        protected String Ip { get; private set; }
        protected int Port { get; private set; }
        protected int SlaveId { get; private set; }
        /// <summary>
        /// 状态机
        /// </summary>
        protected SlaveChannelStatus SlaveChannelStatus { get; private set; }
        /// <summary>
        /// 这台机器modbus地址和具体数据的映射关系
        /// </summary>
        protected List<SlaveAttribute> SlaveAttributeList { get; private set; }


        /// <summary>
        /// 用于解析读取可读写线圈值时的委托方法
        /// </summary>
        /// <param name="responseBytes"></param>
        /// <returns></returns>
        protected delegate Dictionary<ushort, bool> ReadWritableCoilResponseParse(byte[] responseBytes);
        /// <summary>
        /// 用于解析读取可读写寄存器值时的委托方法
        /// </summary>
        /// <param name="responseBytes"></param>
        /// <returns></returns>
        protected delegate Dictionary<ushort, ushort> ReadWritableRegisterResponseParse(byte[] responseBytes);
        /// <summary>
        /// 用于解析读取只读线圈时的委托方法
        /// </summary>
        /// <param name="responseBytes"></param>
        /// <returns></returns>
        protected delegate Dictionary<ushort, bool> ReadReadOnlyCoilResponseParse(byte[] responseBytes);
        /// <summary>
        /// 用于解析读取只读寄存器时的委托方法
        /// </summary>
        /// <param name="responseBytes"></param>
        /// <returns></returns>
        protected delegate Dictionary<ushort, ushort> ReadReadOnlyRegisterResponseParse(byte[] responseBytes);

        /// <summary>
        /// 把ip和端口组合成ip:port的格式
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        protected static String CombineAddress(String ip, long port)
        {
            return ip + ":" + port.ToString();
        }

        public ModbusSlave(String ip, int port, int slaveId, List<SlaveAttribute> slaveAttributeList)
        {
            this.Ip = ip;
            this.Port = port;
            this.SlaveId = slaveId;
            this.SlaveAttributeList = slaveAttributeList;
        }

        public ModbusSlave(String ip, int port, int slaveId, string catagoryName)
        {
            this.Ip = ip;
            this.Port = port;
            this.SlaveId = slaveId;
            this.SlaveAttributeList = SlaveManager.GetModbusMapping(catagoryName);
        }

        /// <summary>
        /// 读可读写线圈
        /// </summary>
        /// <param name="startAddress">查询开始地址</param>
        /// <param name="length">查询个数</param>
        /// <returns>地址和布尔值的映射</returns>
        protected async Task<Dictionary<ushort, bool>> ReadWritableCoil(ushort startAddress, ushort length, ReadWritableCoilResponseParse readWritableCoilResponseParse)
        {
            try
            {
                MemoryStream memoryStream = new MemoryStream();
                byte[] addressBytes = NumberBaseConvertor.SplitUshort2Byte(startAddress);
                byte[] lengthBytes = NumberBaseConvertor.SplitUshort2Byte(length);


                // 获取两个byte作为事务id
                byte[] transactionBytes = TransactionIdGenerator.getTransactionIdBytesByIp(Ip, Port);
                byte[] requestMbapHead = new byte[7] { transactionBytes[0], transactionBytes[1], 0x00, 0x00, 0x00, 0x06, (byte)SlaveId };
                byte[] requestBody = new byte[5] { ModbusConstants.ModbusFunctionCodeReadCoil, addressBytes[0], addressBytes[1], lengthBytes[0], lengthBytes[1] };

                memoryStream.Write(requestMbapHead, 0, requestMbapHead.Length);
                memoryStream.Write(requestBody, 0, requestBody.Length);
                byte[] requestBytes = memoryStream.ToArray();
                // 发起请求，拿到响应报文
                ModbusRequestResult modbudsRequestResult = await ModbusTCPClientManager.ModbusRequestByBytes(Ip, Port, requestBytes);
                // 同步从站状态
                this.SlaveChannelStatus = modbudsRequestResult.SlaveChannelStatus;

                if (modbudsRequestResult.SlaveRequestResult != SlaveRequestResult.Success)
                {
                    return null;
                }

                byte[] responseBytes = modbudsRequestResult.ResponseBytes;


                // 如果有委托方法，直接使用委托方法解析，否则用后续默认方法解析
                if (readWritableCoilResponseParse != null)
                {
                    return readWritableCoilResponseParse(responseBytes);
                }
                else
                {
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
            }
            catch (Exception e)
            {
                // TODO 异常
                return null;
            }


        }

        /// <summary>
        /// 重载方法，不用委托方法用默认方式解析响应数据
        /// </summary>
        /// <param name="startAddress"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        protected async Task<Dictionary<ushort, bool>> ReadWritableCoil(ushort startAddress, ushort length)
        {
            return await ReadWritableCoil(startAddress, length, null);
        }

        /// <summary>
        /// 读保持寄存器（可读写寄存器）
        /// </summary>
        /// <param name="startAddress">查询开始地址</param>
        /// <param name="length">查询个数</param>
        /// <returns>地址和寄存器值的映射</returns>
        protected async Task<Dictionary<ushort, ushort>> ReadWritableRegister(ushort startAddress, ushort dataLengh, ReadWritableRegisterResponseParse readWritableRegisterResponseParse)
        {
            try
            {
                MemoryStream memoryStream = new MemoryStream();
                byte[] addressBytes = NumberBaseConvertor.SplitUshort2Byte(startAddress);
                byte[] lengthBytes = NumberBaseConvertor.SplitUshort2Byte(dataLengh);


                // 获取两个byte作为事务id
                byte[] transactionBytes = TransactionIdGenerator.getTransactionIdBytesByIp(Ip, Port);
                byte[] requestMbapHead = new byte[7] { transactionBytes[0], transactionBytes[1], 0x00, 0x00, 0x00, 0x06, (byte)SlaveId };
                byte[] requestBody = new byte[5] { ModbusConstants.ModbusFunctionCodeReadHoldingRegister, addressBytes[0], addressBytes[1], lengthBytes[0], lengthBytes[1] };

                memoryStream.Write(requestMbapHead, 0, requestMbapHead.Length);
                memoryStream.Write(requestBody, 0, requestBody.Length);
                byte[] requestBytes = memoryStream.ToArray();
                // 发起请求，拿到响应报文
                ModbusRequestResult modbudsRequestResult = await ModbusTCPClientManager.ModbusRequestByBytes(Ip, Port, requestBytes);
                // 同步从站状态
                this.SlaveChannelStatus = modbudsRequestResult.SlaveChannelStatus;

                if (modbudsRequestResult.SlaveRequestResult != SlaveRequestResult.Success)
                {
                    return null;
                }

                byte[] responseBytes = modbudsRequestResult.ResponseBytes;

                // 如果有委托方法，直接使用委托方法解析，否则用后续默认方法解析
                if (readWritableRegisterResponseParse != null)
                {
                    return readWritableRegisterResponseParse(responseBytes);
                }
                else
                {
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
                    for (int i = 0; i < dataBytes.Length; i += 2)
                    {
                        ushort result = NumberBaseConvertor.CombineTwoByte2Ushrot(dataBytes[i], dataBytes[i + 1]);
                        resultKV.Add(startAddress++, result);
                    }

                    return resultKV;

                }
            }
            catch (Exception e)
            {
                // TODO 异常
                return new Dictionary<ushort, ushort>();
            }
        }

        /// <summary>
        /// 重载方法，不用委托方法用默认方式解析响应数据
        /// </summary>
        /// <param name="startAddress"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        protected async Task<Dictionary<ushort, ushort>> ReadWritableRegister(ushort startAddress, ushort dataLengh)
        {
            return await ReadWritableRegister(startAddress, dataLengh, null);
        }

        /// <summary>
        /// 写单个线圈
        /// </summary>
        /// <param name="writeAddress"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        protected async Task<bool> WriteSingleCoil(ushort writeAddress, bool input)
        {
            try
            {
                byte[] addressBytes = NumberBaseConvertor.SplitUshort2Byte(writeAddress);

                byte[] transactionBytes = TransactionIdGenerator.getTransactionIdBytesByIp(Ip, Port);
                byte[] requestMbapHead = new byte[7] { transactionBytes[0], transactionBytes[1], 0x00, 0x00, 0x00, 0x06, (byte)SlaveId };
                byte[] requestBody = new byte[5] { ModbusConstants.ModbusFunctionCodeWriteSingleCoil, addressBytes[0], addressBytes[1], (byte)(input ? 0xFF : 0x00), 0x00 };

                MemoryStream memoryStream = new MemoryStream();
                memoryStream.Write(requestMbapHead, 0, requestMbapHead.Length);
                memoryStream.Write(requestBody, 0, requestBody.Length);

                byte[] requestBytes = memoryStream.ToArray();
                // 发起请求，拿到响应报文
                ModbusRequestResult modbudsRequestResult = await ModbusTCPClientManager.ModbusRequestByBytes(Ip, Port, requestBytes);
                // 同步从站状态
                this.SlaveChannelStatus = modbudsRequestResult.SlaveChannelStatus;


                if (modbudsRequestResult.SlaveRequestResult != SlaveRequestResult.Success)
                {
                    return false;
                }

                byte[] responseBytes = modbudsRequestResult.ResponseBytes;


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
        /// <param name="writeStartAddress"></param>
        /// <param name="dataLengh"></param>
        /// <param name="writeData"></param>
        /// <returns></returns>
        protected async Task<bool> WriteMultiCoil(ushort writeStartAddress, ushort writeLenght, byte[] writeData)
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
                byte[] lengthBytes = NumberBaseConvertor.SplitUshort2Byte(writeLenght);


                // 获取两个byte作为事务id
                byte[] transactionBytes = TransactionIdGenerator.getTransactionIdBytesByIp(Ip, Port);
                // 数据长度是 slaveId(1字节) 功能码(1字节) 起始地址(2字节) 数量(2字节) 字节数(1字节)数据(n字节)
                byte[] requestMbapHead = new byte[7] { transactionBytes[0], transactionBytes[1], 0x00, 0x00, 0x00, (byte)(7 + writeData.Length), (byte)SlaveId };

                // 拼接请求报文
                MemoryStream requestMemoryStream = new MemoryStream();
                requestMemoryStream.Write(requestMbapHead, 0, requestMbapHead.Length);
                requestMemoryStream.Write(new byte[6] { ModbusConstants.ModbusFunctionCodeWriteMultipleCoils, addressBytes[0], addressBytes[1], lengthBytes[0], lengthBytes[1], (byte)writeData.Length }, 0, 6);
                requestMemoryStream.Write(writeData, 0, writeData.Length);
                byte[] requestBytes = requestMemoryStream.ToArray();

                // 发起请求，拿到响应报文
                ModbusRequestResult modbudsRequestResult = await ModbusTCPClientManager.ModbusRequestByBytes(Ip, Port, requestBytes);
                // 同步从站状态
                this.SlaveChannelStatus = modbudsRequestResult.SlaveChannelStatus;

                if (modbudsRequestResult.SlaveRequestResult != SlaveRequestResult.Success)
                {
                    return false;
                }

                byte[] responseBytes = modbudsRequestResult.ResponseBytes;


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
        /// 重载方法，直接用boolList入参，减少烧脑
        /// </summary>
        /// <param name="writeStartAddress"></param>
        /// <param name="boolList"></param>
        /// <returns></returns>
        protected async Task<bool> WriteMultiCoil(ushort writeStartAddress, ushort writeLenght, params bool[] boolList)
        {
            byte[] toBeWriteBytes = NumberBaseConvertor.BoolList2ByteArray(boolList);
            return await WriteMultiCoil(writeStartAddress, writeLenght, toBeWriteBytes);

        }

        /// <summary>
        /// 写单个保持寄存器
        /// </summary>
        /// <param name="writeAddress"></param>
        /// <param name="writeData"></param>
        /// <returns></returns>
        protected async Task<bool> WriteSingleRegister(ushort writeAddress, ushort writeData)
        {
            try
            {
                byte[] addressBytes = NumberBaseConvertor.SplitUshort2Byte(writeAddress);
                byte[] writeDataBytes = NumberBaseConvertor.SplitUshort2Byte(writeData);

                byte[] transactionBytes = TransactionIdGenerator.getTransactionIdBytesByIp(Ip, Port);
                byte[] requestMbapHead = new byte[7] { transactionBytes[0], transactionBytes[1], 0x00, 0x00, 0x00, 0x06, (byte)SlaveId };
                byte[] requestBody = new byte[5] { ModbusConstants.ModbusFunctionCodeWriteSingleRegister, addressBytes[0], addressBytes[1], writeDataBytes[0], writeDataBytes[1] };

                MemoryStream memoryStream = new MemoryStream();
                memoryStream.Write(requestMbapHead, 0, requestMbapHead.Length);
                memoryStream.Write(requestBody, 0, requestBody.Length);

                byte[] requestBytes = memoryStream.ToArray();
                // 发起请求，拿到响应报文
                ModbusRequestResult modbudsRequestResult = await ModbusTCPClientManager.ModbusRequestByBytes(Ip, Port, requestBytes);
                // 同步从站状态
                this.SlaveChannelStatus = modbudsRequestResult.SlaveChannelStatus;


                if (modbudsRequestResult.SlaveRequestResult != SlaveRequestResult.Success)
                {
                    return false;
                }

                byte[] responseBytes = modbudsRequestResult.ResponseBytes;


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
        /// <param name="writeStartAddress"></param>
        /// <param name="dataLengh"></param>
        /// <param name="writeData"></param>
        /// <returns></returns>
        protected async Task<bool> WriteMultiRegister(ushort writeStartAddress, ushort writeLength, ushort[] writeData)
        {
            try
            {
                // 校验
                if (writeData.Length > 123)
                {
                    throw new ArgumentException("写入多个寄存器数据时，数据超长，最多同时写入123个寄存器)");
                }

                if (writeData.Length != writeLength)
                {
                    throw new ArgumentException("写入多个寄存器数据时，writeLength和writeData的长度不一致");
                }

                MemoryStream memoryStream = new MemoryStream();
                byte[] addressBytes = NumberBaseConvertor.SplitUshort2Byte(writeStartAddress);
                byte[] lengthBytes = NumberBaseConvertor.SplitUshort2Byte((writeLength));


                // 获取两个byte作为事务id
                byte[] transactionBytes = TransactionIdGenerator.getTransactionIdBytesByIp(Ip, Port);
                byte[] requestMbapHead = new byte[7] { transactionBytes[0], transactionBytes[1], 0x00, 0x00, 0x00, (byte)(7 + (writeData.Length * 2)), (byte)SlaveId };

                // 拼接请求报文
                MemoryStream requestMemoryStream = new MemoryStream();
                requestMemoryStream.Write(requestMbapHead, 0, requestMbapHead.Length);
                requestMemoryStream.Write(new byte[6] { ModbusConstants.ModbusFunctionCodeWriteMultipleRegisters, addressBytes[0], addressBytes[1], lengthBytes[0], lengthBytes[1], (byte)(writeData.Length * 2) }, 0, 6);
                byte[] writeDataBytes = NumberBaseConvertor.UshortArray2ByteArray(writeData);
                requestMemoryStream.Write(writeDataBytes, 0, writeDataBytes.Length);
                byte[] requestBytes = requestMemoryStream.ToArray();

                // 发起请求，拿到响应报文
                ModbusRequestResult modbudsRequestResult = await ModbusTCPClientManager.ModbusRequestByBytes(Ip, Port, requestBytes);
                // 同步从站状态
                this.SlaveChannelStatus = modbudsRequestResult.SlaveChannelStatus;

                if (modbudsRequestResult.SlaveRequestResult != SlaveRequestResult.Success)
                {
                    return false;
                }

                byte[] responseBytes = modbudsRequestResult.ResponseBytes;


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
        /// <param name="startAddress"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        protected async Task<Dictionary<ushort, bool>> ReadReadOnlyCoil(ushort startAddress, ushort length, ReadReadOnlyCoilResponseParse readOnlyCoilResponseParse)
        {
            try
            {
                MemoryStream memoryStream = new MemoryStream();
                byte[] addressBytes = NumberBaseConvertor.SplitUshort2Byte(startAddress);
                byte[] lengthBytes = NumberBaseConvertor.SplitUshort2Byte(length);


                // 获取两个byte作为事务id
                byte[] transactionBytes = TransactionIdGenerator.getTransactionIdBytesByIp(Ip, Port);
                byte[] requestMbapHead = new byte[7] { transactionBytes[0], transactionBytes[1], 0x00, 0x00, 0x00, 0x06, (byte)SlaveId };
                byte[] requestBody = new byte[5] { ModbusConstants.ModbusFunctionCodeReadDiscreteInputs, addressBytes[0], addressBytes[1], lengthBytes[0], lengthBytes[1] };

                memoryStream.Write(requestMbapHead, 0, requestMbapHead.Length);
                memoryStream.Write(requestBody, 0, requestBody.Length);
                byte[] requestBytes = memoryStream.ToArray();
                // 发起请求，拿到响应报文
                ModbusRequestResult modbudsRequestResult = await ModbusTCPClientManager.ModbusRequestByBytes(Ip, Port, requestBytes);
                // 同步从站状态
                this.SlaveChannelStatus = modbudsRequestResult.SlaveChannelStatus;

                if (modbudsRequestResult.SlaveRequestResult != SlaveRequestResult.Success)
                {
                    return null;
                }

                byte[] responseBytes = modbudsRequestResult.ResponseBytes;


                // 如果有委托方法，直接使用委托方法解析，否则用后续默认方法解析
                if (readOnlyCoilResponseParse != null)
                {
                    return readOnlyCoilResponseParse(responseBytes);
                }
                else
                {
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
            }
            catch (Exception e)
            {
                // TODO 异常
                return new Dictionary<ushort, bool>();
            }

        }

        protected async Task<Dictionary<ushort, bool>> ReadReadOnlyCoil(ushort startAddress, ushort length)
        {
            return await ReadReadOnlyCoil(startAddress, length, null);
        }


        /// <summary>
        /// 读只读寄存器（输入寄存器）
        /// </summary>
        /// <param name="startAddress"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        protected async Task<Dictionary<ushort, ushort>> ReadReadOnlyRegister(ushort startAddress, ushort dataLengh, ReadReadOnlyRegisterResponseParse readOnlyRegisterResponseParse)
        {
            try
            {
                MemoryStream memoryStream = new MemoryStream();
                byte[] addressBytes = NumberBaseConvertor.SplitUshort2Byte(startAddress);
                byte[] lengthBytes = NumberBaseConvertor.SplitUshort2Byte(dataLengh);


                // 获取两个byte作为事务id
                byte[] transactionBytes = TransactionIdGenerator.getTransactionIdBytesByIp(Ip, Port);
                byte[] requestMbapHead = new byte[7] { transactionBytes[0], transactionBytes[1], 0x00, 0x00, 0x00, 0x06, (byte)SlaveId };
                byte[] requestBody = new byte[5] { ModbusConstants.ModbusFunctionCodeReadInputRegisters, addressBytes[0], addressBytes[1], lengthBytes[0], lengthBytes[1] };

                memoryStream.Write(requestMbapHead, 0, requestMbapHead.Length);
                memoryStream.Write(requestBody, 0, requestBody.Length);
                byte[] requestBytes = memoryStream.ToArray();
                // 发起请求，拿到响应报文
                ModbusRequestResult modbudsRequestResult = await ModbusTCPClientManager.ModbusRequestByBytes(Ip, Port, requestBytes);
                // 同步从站状态
                this.SlaveChannelStatus = modbudsRequestResult.SlaveChannelStatus;

                if (modbudsRequestResult.SlaveRequestResult != SlaveRequestResult.Success)
                {
                    return null;
                }

                byte[] responseBytes = modbudsRequestResult.ResponseBytes;



                // 如果有委托方法，直接使用委托方法解析，否则用后续默认方法解析
                if (readOnlyRegisterResponseParse != null)
                {
                    return readOnlyRegisterResponseParse(responseBytes);
                }
                else
                {
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
                    for (int i = 0; i < dataBytes.Length; i += 2)
                    {
                        ushort result = NumberBaseConvertor.CombineTwoByte2Ushrot(dataBytes[i], dataBytes[i + 1]);
                        resultKV.Add(startAddress++, result);
                    }

                    return resultKV;

                }
            }
            catch (Exception e)
            {
                // TODO 异常
                return new Dictionary<ushort, ushort>();
            }
        }

        protected async Task<Dictionary<ushort, ushort>> ReadReadOnlyRegister(ushort startAddress, ushort dataLengh)
        {
            return await ReadReadOnlyRegister(startAddress, dataLengh, null);
        }

        protected void FillResultInMapping<T>(Dictionary<ushort, T> result, List<SlaveAttribute> modbusMappingList)
        {
            // 将获取到的结果填充到_modbusMappingList
            foreach (SlaveAttribute addressInfo in modbusMappingList)
            {
                ushort address = addressInfo.Address;
                if (result.ContainsKey(address))
                {
                    addressInfo.Value = result[address];
                }
            }

        }

        protected SlaveAttribute GetAttributeByRegionAndAddress(int region, ushort address )
        {
            foreach (SlaveAttribute attribute in SlaveAttributeList)
            {
                if (address == attribute.Address && region == attribute.Region)
                {
                    return attribute;
                }
            }
            string errorMsg = $"从站{Ip}:{Port}-{SlaveId}获取地址${address}时失败，可能Modbus协议配置中没有改地址";
            SimpleLogger.Instance.Error(errorMsg);
            throw new ArgumentException(errorMsg);


        }

        abstract public void RefreshData();
    }
}
