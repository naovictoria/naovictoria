using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;
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

                if (LogWriter != null)
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

        public static async Task<string> ReadLineAsync(
            this SerialPort serialPort, CancellationToken token)
        {
            byte[] buffer = new byte[1];
            string ret = string.Empty;

            while (!token.IsCancellationRequested)
            {
                await serialPort.BaseStream.ReadAsync(buffer, 0, 1, token);
                ret += serialPort.Encoding.GetString(buffer);

                if (LogWriter != null)
                {
                    await LogWriter.WriteAsync((char)buffer[0]);
                    await LogWriter.FlushAsync();
                }

                Console.Write((char)buffer[0]);

                if (ret.EndsWith(serialPort.NewLine))
                    // Truncate the line ending
                    return ret.Substring(0, ret.Length - serialPort.NewLine.Length);
            }

            return String.Empty;
        }

        public static async Task WriteLineAsync(
            this SerialPort serialPort, string str)
        {
            byte[] encodedStr =
                serialPort.Encoding.GetBytes(str + serialPort.NewLine);

            await serialPort.BaseStream.WriteAsync(encodedStr, 0, encodedStr.Length);
            await serialPort.BaseStream.FlushAsync();
        }

        public static async Task WriteLineAsync(
            this SerialPort serialPort, string str, CancellationToken token)
        {
            byte[] encodedStr =
                serialPort.Encoding.GetBytes(str + serialPort.NewLine);

            Console.WriteLine("sending: " + str);

            await serialPort.BaseStream.WriteAsync(encodedStr, 0, encodedStr.Length, token);
            await serialPort.BaseStream.FlushAsync(token);
        }

        public static async Task WriteAsync(
            this SerialPort serialPort, byte[] buffer, int offset, int count)
        {
            await serialPort.BaseStream.WriteAsync(buffer, offset, count);
            await serialPort.BaseStream.FlushAsync();
        }

        public static async Task WriteAsync(
            this SerialPort serialPort, byte[] buffer, int offset, int count, CancellationToken token)
        {
            await serialPort.BaseStream.WriteAsync(buffer, offset, count, token);
            await serialPort.BaseStream.FlushAsync(token);
        }

        public static async Task<string> SendCommandAsync(
            this SerialPort serialPort, string command, int responseLines, int returnLineNum)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            cts.CancelAfter(1000);

            await serialPort.WriteLineAsync(command, cts.Token);

            string line = string.Empty;
            int retryCount = 0;

            while (line.Trim() != command.Trim())
            {
                try
                {
                    line = await serialPort.ReadLineAsync(cts.Token);
                    Console.WriteLine("response1: " + line);
                }
                catch (TaskCanceledException ex)
                {
                    retryCount++;
                    if (retryCount > 5) { throw new Exception("Not connected."); }
                    cts = new CancellationTokenSource();
                    cts.CancelAfter(1000);
                    await serialPort.WriteLineAsync(command, cts.Token);
                }
            }

            string returnLine = string.Empty;

            Console.WriteLine("Response Lines: " + responseLines);

            for (int i = 0; i < responseLines; i++)
            {
                line = await serialPort.ReadLineAsync();
                Console.WriteLine("response2: " + line);
                if (i + 1 == returnLineNum)
                {
                    returnLine = line;
                }
            }
            return returnLine;
        }

        public static async Task<string> SendCommandAsync(
            this SerialPort serialPort, string command, int responseLines, int returnLineNum, CancellationToken cts)
        {
            await serialPort.WriteLineAsync(command, cts);

            string returnLine = string.Empty;
            for (int i = 0; i < responseLines; i++)
            {
                if (i + 1 == returnLineNum)
                {
                    returnLine = await serialPort.ReadLineAsync(cts);
                }
                else
                {
                    await serialPort.ReadLineAsync(cts);
                }
            }
            return returnLine;
        }
    }
}
