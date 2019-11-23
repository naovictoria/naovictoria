using System;

namespace RFM9X.Playground
{
    class Program
    {
        static void Main(string[] args)
        {
            double radioFreqMhz = 915.0;
            RFM9X rfm9x = new RFM9X(radioFreqMhz);
        }
    }
}
