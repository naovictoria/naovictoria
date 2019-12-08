using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Iot.Device.RadioTransceiver;
using NaoVictoria.Models;

namespace NaoVictoria
{
    public class LoraTelemetrySender : ITelemetrySender
    {
        private long _lastTelemetrySent;

        public async Task SendTelementry(TelemetryData data)
        {
            if (_lastTelemetrySent + 1 >= DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            {
                return;
            }

            _lastTelemetrySent = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            byte[] telemetryBuffer = new byte[45];
            
            // Signature
            telemetryBuffer[0] = 0x87;
            int bc = 1;
            
            BitConverter.GetBytes((uint)data.Timestamp).CopyTo(telemetryBuffer, bc); bc += sizeof(uint);
            BitConverter.GetBytes((float)data.Longitude).CopyTo(telemetryBuffer, bc); bc += sizeof(float);
            BitConverter.GetBytes((float)data.Latitude).CopyTo(telemetryBuffer, bc); bc += sizeof(float);
            BitConverter.GetBytes((float)data.OrientationHeading).CopyTo(telemetryBuffer, bc); bc += sizeof(float);
            BitConverter.GetBytes((float)data.OrientationPitch).CopyTo(telemetryBuffer, bc); bc += sizeof(float);
            BitConverter.GetBytes((float)data.OrientationRoll).CopyTo(telemetryBuffer, bc); bc += sizeof(float);
            BitConverter.GetBytes(data.BowDistanceToCollisionCm).CopyTo(telemetryBuffer, bc); bc += sizeof(int);

            Console.WriteLine(BitConverter.ToString(telemetryBuffer));

            int radioFreqMhz = 915;
            RFM9X rfm9x = new RFM9X(radioFreqMhz);
            rfm9x.Send(telemetryBuffer);

            return;
}
    }
}
