namespace FArduino
{
    public class ArduinoChecksum
    {
        public static byte CalculateCheckSum(byte[] data)
        {
            byte rtn = 0;
            for (int i = 0; i < data.Length; i++)
            {
                rtn += data[i];
            }
            return (byte)(0xFF - rtn + 1);
        }
    }
}
