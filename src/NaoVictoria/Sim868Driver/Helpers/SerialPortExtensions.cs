using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading.Tasks;

namespace Sim868HttpClient.Helpers
{
    public static class SerialPortExtensions
    {
        public static StreamWriter LogWriter = null;

        public static async Task<string> ReadLineAsync(
            this SerialPort serialPort)
        {
            byte[] buffer = new byte[1];
            string ret = string.Empty;

            while (true)
            {
                await serialPort.BaseStream.ReadAsync(buffer, 0, 1);
                ret += serialPort.Encoding.GetString(buffer);

                if(LogWriter != null)
                {
                    await LogWriter.WriteAsync((char)buffer[0]);
                    await LogWriter.FlushAsync();
                }

                Console.Write((char)buffer[0]);

                if (ret.EndsWith(serialPort.NewLine))
                    // Truncate the line ending
                    return ret.Substring(0, ret.Length - serialPort.NewLine.Length);
            }
        }

        public static async Task WriteLineAsync(
            this SerialPort serialPort, string str)
        {
            byte[] encodedStr =
                serialPort.Encoding.GetBytes(str + serialPort.NewLine);

            await serialPort.BaseStream.WriteAsync(encodedStr, 0, encodedStr.Length);
            await serialPort.BaseStream.FlushAsync();
        }

        public static async Task WriteAsync(
            this SerialPort serialPort, byte[] buffer, int offset, int count)
        {
            await serialPort.BaseStream.WriteAsync(buffer, offset, count);
            await serialPort.BaseStream.FlushAsync();
        }

        public static async Task<string> SendCommandAsync(
            this SerialPort serialPort, string command, int responseLines, int returnLineNum)
        {
            await serialPort.WriteLineAsync(command);

            string returnLine = string.Empty;
            for (int i = 0; i < responseLines; i++)
            {
                if (i + 1 == returnLineNum)
                {
                    returnLine = await serialPort.ReadLineAsync();
                } else
                {
                    await serialPort.ReadLineAsync();
                }
            }
            return returnLine;
        }
    }
}
