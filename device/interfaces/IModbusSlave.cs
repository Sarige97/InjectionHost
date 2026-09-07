using moju.constants;
using moju.domain;
using moju.log;
using moju.modbus;
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
    internal interface IModbusSlave
    {
        /// <summary>
        /// 重载方法，不用委托方法用默认方式解析响应数据
        /// </summary>
        /// <param name="startAddress"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        Task<Dictionary<ushort, bool>> ReadWritableCoil(ushort startAddress, ushort length);

        /// <summary>
        /// 重载方法，不用委托方法用默认方式解析响应数据
        /// </summary>
        /// <param name="startAddress"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        Task<Dictionary<ushort, ushort>> ReadWritableRegister(ushort startAddress, ushort dataLengh);

        /// <summary>
        /// 写单个线圈
        /// </summary>
        /// <param name="writeAddress"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        Task<bool> WriteSingleCoil(ushort writeAddress, bool input);

        /// <summary>
        /// 写多个线圈（用字节作为写入结果）
        /// </summary>
        /// <param name="writeStartAddress"></param>
        /// <param name="dataLengh"></param>
        /// <param name="writeData"></param>
        /// <returns></returns>
        Task<bool> WriteMultiCoil(ushort writeStartAddress, ushort writeLenght, byte[] writeData);

        /// <summary>
        /// 重载方法，直接用boolList入参，减少烧脑
        /// </summary>
        /// <param name="writeStartAddress"></param>
        /// <param name="boolList"></param>
        /// <returns></returns>
        Task<bool> WriteMultiCoil(ushort writeStartAddress, ushort writeLenght, params bool[] boolList);

        /// <summary>
        /// 写单个保持寄存器
        /// </summary>
        /// <param name="writeAddress"></param>
        /// <param name="writeData"></param>
        /// <returns></returns>
        Task<bool> WriteSingleRegister(ushort writeAddress, ushort writeData);

        /// <summary>
        ///  写多个保持寄存器
        /// </summary>
        /// <param name="writeStartAddress"></param>
        /// <param name="dataLengh"></param>
        /// <param name="writeData"></param>
        /// <returns></returns>
        Task<bool> WriteMultiRegister(ushort writeStartAddress, ushort writeLength, ushort[] writeData);

        Task<Dictionary<ushort, bool>> ReadReadOnlyCoil(ushort startAddress, ushort length);

        Task<Dictionary<ushort, ushort>> ReadReadOnlyRegister(ushort startAddress, ushort dataLengh);

        void RefreshData();

    }
}
