using System;
using System.Collections.Generic;
using System.Text;

namespace FArduino
{
    // Packet => Payload =(encapsulate)=> Frame =(decapsultae)=> Payload => Packet
    public class ArduinoPacketConverter: IPacketConverter<ArduinoPacket>
    {
        public byte STX = 0x02;
        public byte ETX1 = 0x0D; // Carriage Return
        public byte ETX2 = 0x0A; // Line Feed
        public byte WHITESPACE = 0x20; // White Space

        public bool UseChecksum { get; set; } = true;
        public bool UseSTX { get; set; } = true;

        public byte[] ConvertPacketToFrame(ArduinoPacket packet)
        {
            var payload = ConvertPacketToPayload(packet);
            var frame = EncapsulatePayload(payload);
            return frame.ToArray();
        }

        public ArduinoPacket ConvertPayloadToPacket(byte[] payload)
        {
            var data = Encoding.ASCII.GetString(payload);

            var tokens = data.Split(' ');
            if (tokens.Length < 0)
            {
                // add Custom Exception
                throw new ArgumentException("not found command in arduino packet");
            }

            var packet = new ArduinoPacket();

            packet.Command = tokens[0];

            if (tokens.Length > 1 && !string.IsNullOrEmpty(tokens[1]))
            {
                packet.Arguments = tokens[1].Split(',');
            }

            return packet;
        }

        public List<byte> ConvertPacketToPayload(ArduinoPacket packet)
        {
            var payload = new List<byte>();
            // Add Command
            var commandBytes = Encoding.ASCII.GetBytes(packet.Command);
            var args = packet.Arguments;

            payload.AddRange(commandBytes);

            // Add Argument Data
            if (args != null && args.Length > 0)
            {
                payload.Add(WHITESPACE);
                for (var i = 0; i < args.Length; i++)
                {
                    payload.AddRange(Encoding.ASCII.GetBytes(args[i]));
                    if (i < args.Length - 1)
                    {
                        payload.Add((byte)',');
                    }
                }
            }
            return payload;
        }

        public List<byte> EncapsulatePayload(List<byte> payload)
        {
            if (UseChecksum)
            {
                // Add CheckSum
                var checkSum = ArduinoChecksum.CalculateCheckSum(payload.ToArray());
                payload.Add(checkSum);
            }

            if (UseSTX)
            {
                // Add STX in payload
                payload.Insert(0, STX);
            }

            // Add ETX
            payload.Add(ETX1);
            payload.Add(ETX2);
            return payload;
        }

        public bool DecapsulateFrame(byte[] buf, out byte[] payload)
        {
            // Find Data Index

            int stxIdx = -1;
            if (UseSTX)
            {
                // Find STX Index in buffer.
                for (var idx = 0; idx < buf.Length; idx++)
                {
                    if (buf[idx] == STX)
                    {
                        stxIdx = idx;
                        break;
                    }
                }
                // Check Whether No Exist STX
                if (stxIdx == -1)
                {
                    payload = null;
                    return false;
                }
            }

            var dataIdx = stxIdx + 1;

            // Find ETX Index in buffer.
            int etxIdx = -1;
            for (var idx = dataIdx; idx < buf.Length - 1; idx++)
            {
                if (buf[idx] == ETX1 && buf[idx + 1] == ETX2)
                {
                    etxIdx = idx;
                    break;
                }
            }

            // Check Whether No Exist ETX
            if (etxIdx == -1)
            {
                payload = null;
                return false;
            }

            var arraySpan = new Span<byte>(buf);
            if (UseChecksum)
            {
                // frame => ... [stx(1)] [data(n)] [checksum(1)] [etx(2)] ...
                var checkSumIdx = etxIdx - 1;
                var data = arraySpan.Slice(start: dataIdx, length: checkSumIdx - dataIdx); // Extract data from frame
                var checkSum = arraySpan[checkSumIdx];

                var expectedChecksum = ArduinoChecksum.CalculateCheckSum(data.ToArray());
                // Check Checksum Value
                if (expectedChecksum == checkSum)
                {
                    payload = data.ToArray();
                    return true;
                }
                else
                {
                    // Throw Checksum error
                    payload = null;
                    return false;
                }
            }
            else
            {
                var data = arraySpan.Slice(start: dataIdx, length: etxIdx - dataIdx); // Extract data from frame
                payload = data.ToArray();
                return true;
            }
        }
    }
}
