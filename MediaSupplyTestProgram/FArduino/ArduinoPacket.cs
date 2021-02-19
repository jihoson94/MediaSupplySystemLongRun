using OneTrio.Core;

namespace FArduino
{

    public struct ArduinoPacket:IPacket
    {
        public string Command { get; set; }
        public string[] Arguments { get; set; }
    }
}
