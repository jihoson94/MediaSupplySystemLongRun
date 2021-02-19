using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FArduino
{

    // T: Parsed Data Type (usually byte or string)
    public abstract class SyncSerialDriver<T>
    {
        public SerialPort SerialPort { get; set; }

        private int _receiveTimoutMillisecond = 500;
        private int _maxRetryCount = 5;

        public SyncSerialDriver(int maxRetryCount = 5, int receiveTimoutMillisecond = 500)
        {
            _receiveTimoutMillisecond = receiveTimoutMillisecond;
            _maxRetryCount = maxRetryCount;
        }

        public bool Connect()
        {
            try
            {
                SerialPort.Close(); // for refrash serial port.
                SerialPort.Open();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void Disconnect()
        {
            SerialPort.Close();
        }

        public bool IsConnected()
        {
            return SerialPort.IsOpen;
        }

        public void ChangeSerialPort(string portName, int buadRate)
        {
            if (buadRate <= 0)
            {
                throw new ArgumentException("buadrate require positive");
            }

            if (String.IsNullOrEmpty(portName))
            {
                throw new ArgumentException("PortName must be not Null");
            }

            if (SerialPort != null)
            {
                SerialPort.Close();
            }

            SerialPort = new SerialPort(portName, buadRate);
            SerialPort.Encoding = Encoding.GetEncoding(28591);
            SerialPort.RtsEnable = true;
            SerialPort.DtrEnable = true;
        }

        #region Synchronuous Send/Recv

        private object _lock = new object();

        public async Task<T> SendByteAsync(byte[] bytes)
        {
            int retryCount = 0;
            while (retryCount < _maxRetryCount)
            {
                retryCount++;

                var cts = new CancellationTokenSource(_receiveTimoutMillisecond * 2);
                // pass cts.Token to async APIs
                var jobTask = Task.Run(() =>
                {
                    lock (_lock)
                    {
                        try
                        {
                            SerialPort.Write(bytes, 0, bytes.Length);
                        }
                        catch (InvalidOperationException ex)
                        {
                            throw new Exception(ex.Message);
                        }

                        return WaitReceiveData(cts.Token);
                    }
                });


                if (jobTask != await Task.WhenAny(jobTask, Task.Delay(_receiveTimoutMillisecond)))
                {
                    cts.Cancel();
                    jobTask.Wait();
                    cts.Dispose();
                    continue;
                }

                return await jobTask;
            }

            // When retry count is more than MaxTryCount, Throw Communication Timeout Exception.
            var sentData = Encoding.ASCII.GetString(bytes).Trim('\n').Trim('\r');
            throw new Exception("Not receive about data(" + sentData + ")");
        }

        private T WaitReceiveData(CancellationToken token)
        {
            // Warning) watch out hang forever serial
            var buffer = new List<byte>();
            while (!token.IsCancellationRequested) // Need timeout with Cancellation Token
            {
                while (SerialPort.BytesToRead > 0) // if byte exist.
                {
                    buffer.Add((byte)SerialPort.ReadByte());
                }

                if (!ExtractData(buf: buffer.ToArray(), out T receivedData))
                {
                    // Watch out exist about infinite loop.
                    continue;
                }
                else
                {
                    return receivedData;
                }
            }

            return default(T); // Exit by Cancel Token.
        }

        public abstract bool ExtractData(byte[] buf, out T receivedData);

        #endregion
    }

}
