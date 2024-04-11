using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace Modbus.ModbusFunctions
{
    /// <summary>
    /// Class containing logic for parsing and packing modbus write single register functions/requests.
    /// </summary>
    public class WriteSingleRegisterFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WriteSingleRegisterFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
        public WriteSingleRegisterFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusWriteCommandParameters));
        }

        /// <inheritdoc />
        public override byte[] PackRequest()
        {
            //TO DO: IMPLEMENT
            ModbusReadCommandParameters modbus = (ModbusReadCommandParameters)CommandParameters;
            byte[] b = new byte[12];

            byte[] trans = BitConverter.GetBytes(modbus.TransactionId);
            b[0] = trans[1];
            b[1] = trans[0];

            byte[] protc = BitConverter.GetBytes(modbus.ProtocolId);
            b[2] = protc[1];
            b[3] = protc[0];

            byte[] length = BitConverter.GetBytes(modbus.Length);
            b[4] = length[1];
            b[5] = length[0];

            b[6] = (byte)(modbus.UnitId);

            b[7] = (byte)(modbus.FunctionCode);

            byte[] start_address = BitConverter.GetBytes(modbus.StartAddress);
            b[8] = start_address[1];
            b[9] = start_address[0];

            byte[] quantity = BitConverter.GetBytes(modbus.Quantity);
            b[10] = quantity[1];
            b[11] = quantity[0];

            return b;
        }

        /// <inheritdoc />
        public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)
        {
            //TO DO: IMPLEMENT
            Dictionary<Tuple<PointType, ushort>, ushort> dic = new Dictionary<Tuple<PointType, ushort>, ushort>();
            ModbusReadCommandParameters modbus = (ModbusReadCommandParameters)CommandParameters;

            if (response.Length <= 9)
            {
                Console.WriteLine("INVALID MESSAGE!");
            }
            else
            {
                for (int i = 0; i < response[8]; i += 2)
                {
                    Tuple<PointType, ushort> t = Tuple.Create(PointType.DIGITAL_OUTPUT, modbus.StartAddress);
                    byte[] bytes = new byte[1];

                    bytes[0] = response[9 + i];
                    string a = "";
                    foreach (byte b in bytes)
                    {
                        string bit = Convert.ToString(b, 2).PadLeft(8, '0');
                        a += bit;
                    }
                    dic.Add(t, (ushort)Convert.ToUInt16(a, 2));
                }
            }

            return dic;
        }
    }
}