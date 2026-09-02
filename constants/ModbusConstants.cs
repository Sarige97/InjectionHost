using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace moju.constants
{
    internal class ModbusConstants
    {

        /// <summary>
        /// 读线圈
        /// </summary>
        public const byte ModbusFunctionCodeReadCoil = 0x01;
        /// <summary>
        /// 读离散输出（只读线圈）
        /// </summary>
        public const byte ModbusFunctionCodeReadDiscreteInputs = 0x02;
        /// <summary>
        /// 读保持寄存器
        /// </summary>
        public const byte ModbusFunctionCodeReadHoldingRegister = 0x03;
        /// <summary>
        /// 读输入寄存器（只读寄存器）
        /// </summary>
        public const byte ModbusFunctionCodeReadInputRegisters = 0x04;
        /// <summary>
        /// 写单个线圈
        /// </summary>
        public const byte ModbusFunctionCodeWriteSingleCoil = 0x05;
        /// <summary>
        /// 写单个寄存器
        /// </summary>
        public const byte ModbusFunctionCodeWriteSingleRegister = 0x06;
        /// <summary>
        /// 写多个线圈
        /// </summary>
        public const byte ModbusFunctionCodeWriteMultipleCoils = 0x0F;
        /// <summary>
        /// 写多个寄存器
        /// </summary>
        public const byte ModbusFunctionCodeWriteMultipleRegisters = 0x10;
    }
}
