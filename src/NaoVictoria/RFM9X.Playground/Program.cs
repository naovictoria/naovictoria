using System;

namespace RFM9X.Playground
{
    class Program
    {
        static void Main(string[] args)
        {
            int radioFreqMhz = 915;
            RFM9X rfm9x = new RFM9X(radioFreqMhz);

            rfm9x.Receive();
        }
    }
}
