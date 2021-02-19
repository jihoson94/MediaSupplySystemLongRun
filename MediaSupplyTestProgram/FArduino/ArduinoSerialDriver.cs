using FArduino;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FArduino { 
    public class ArduinoSerialDriver : SyncSerialDriver<byte[]>
    {
        private readonly ArduinoPacketConverter packetConverter;

        public ArduinoSerialDriver(bool useChecksum = true, bool useSTX = true)
        {
            packetConverter = new ArduinoPacketConverter();
            packetConverter.UseChecksum = useChecksum;
            packetConverter.UseSTX = useSTX;
        }

        public override bool ExtractData(byte[] buf, out byte[] receivedData)
        {
            // Extract frame from buffer and Decapsulate Frame and return payload
            var isExtract = packetConverter.DecapsulateFrame(buf, out byte[] payload);
            if (isExtract)
            {
                receivedData = payload;
                return true;
            }
            else
            {
                receivedData = null;
                return false;
            }
        }

        public async Task<ArduinoPacket> SendPacketAsync(ArduinoPacket packet)
        {
            var reqeustFrame = packetConverter.ConvertPacketToFrame(packet);
            var responseFrame = await SendByteAsync(reqeustFrame); ;
            return packetConverter.ConvertPayloadToPacket(responseFrame);
        }

        public async Task<byte[]> SendPacketWithRawResponseAsync(ArduinoPacket packet)
        {
            var reqeustFrame = packetConverter.ConvertPacketToFrame(packet);
            return await SendByteAsync(reqeustFrame);
        }
    }
}
