using System.Collections.Generic;

namespace FArduino
{
    public interface IPacketConverter<T> where T : IPacket
    {
        byte[] ConvertPacketToFrame(T packet);
        T ConvertPayloadToPacket(byte[] payload);
        List<byte> ConvertPacketToPayload(T packet);
        List<byte> EncapsulatePayload(List<byte> payload);
        bool DecapsulateFrame(byte[] buf, out byte[] payload);
    }
}
