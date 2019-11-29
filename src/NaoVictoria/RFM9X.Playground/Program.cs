using System;
using System.Text;
using Iot.Device.RadioTransceiver;

namespace RFM9XPlayground
{
    class Program
    {
        static void Main(string[] args)
        {
            int radioFreqMhz = 915;
            RFM9X rfm9x = new RFM9X(radioFreqMhz);

            //while (true)
            //{
            //    try
            //    {
            //        string message = Encoding.ASCII.GetString(rfm9x.Receive());
            //        Console.WriteLine($"Messge received: {message}");
            //    } catch (TimeoutException)
            //    {
            //        Console.WriteLine("No message received.");
            //    }
            //}

            rfm9x.Send(Encoding.ASCII.GetBytes("Wow, this is so cool!"));

        }
    }
}
