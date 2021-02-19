using FArduino;
using System;
using System.Text;
using System.Threading.Tasks;

namespace MediaSupplyTestProgram
{
    public enum MediaSupplySystemMode
    {
        Ready = 0,
        Move = 1,
        Error = 2
    }

    public struct MediaSupplySystemState
    {
        public MediaSupplySystemMode mode;
        public bool origin;
        public string errorCode;
    }

    public class MediaSupplySystemDriver
    {
        private readonly ArduinoSerialDriver _communicationDriver;
        public MediaSupplySystemDriver()
        {
            _communicationDriver = new ArduinoSerialDriver(useChecksum: false, useSTX: false);
        }

        #region Network
        public bool IsConnected()
        {
            return _communicationDriver.IsConnected();
        }

        public bool Connect(string comPort, int buadrate)
        {
            _communicationDriver.ChangeSerialPort(comPort, buadrate);
            return _communicationDriver.IsConnected();
        }

        public void Disconnect()
        {
            _communicationDriver.Disconnect();
        }


        #endregion

        public async Task<MediaSupplySystemState> GetState()
        {
            var responsePacket = await _communicationDriver.SendPacketAsync(new ArduinoPacket()
            {
                Command = ArduinoMediaSupplySystemCommands.GetStatusCommand
            });

            if (responsePacket.Command == ArduinoMediaSupplySystemCommands.GetStatusCommand)
            {
                if (responsePacket.Arguments != null && responsePacket.Arguments.Length == 3)
                {
                    try
                    {
                        return new MediaSupplySystemState()
                        {
                            mode = (MediaSupplySystemMode)int.Parse(responsePacket.Arguments[0]),
                            origin = int.Parse(responsePacket.Arguments[1]) == 1,
                            errorCode = responsePacket.Arguments[2]
                        };
                    }
                    catch (Exception)
                    {
                        throw new Exception($"{nameof(GetState)} call receive invalid packet.(parsing error)");
                    }
                }
            }

            throw new Exception($"{nameof(GetState)} call receive invalid packet. (missing data)");
        }

        public async Task<string> GetErrorCode()
        {
            var responsePacket = await _communicationDriver.SendPacketAsync(new ArduinoPacket()
            {
                Command = ArduinoMediaSupplySystemCommands.GetErrorCodeCommand
            });

            if (responsePacket.Command == ArduinoMediaSupplySystemCommands.GetErrorCodeCommand)
            {
                if (responsePacket.Arguments != null && responsePacket.Arguments.Length == 1)
                {
                    return responsePacket.Arguments[0];
                }
            }
            throw new Exception($"{nameof(GetErrorCode)} call receive invalid packet.");
        }

        public async Task<string> GetErrorDescription()
        {
            var rawResponse = await _communicationDriver.SendPacketWithRawResponseAsync(new ArduinoPacket()
            {
                Command = ArduinoMediaSupplySystemCommands.GetErrorDescriptionCommand
            });

            var data = Encoding.ASCII.GetString(rawResponse);
            var tokens = data.Split(' ');
            if (tokens.Length > 0)
            {
                if (tokens[0] == ArduinoMediaSupplySystemCommands.GetErrorDescriptionCommand)
                {
                    var stringBuilder = new StringBuilder();
                    for (var i = 1; i < tokens.Length; i++)
                    {
                        stringBuilder.Append(tokens[i]);
                        stringBuilder.Append(" ");
                    }
                    return stringBuilder.ToString();
                }
            }

            throw new Exception($"{nameof(GetErrorDescription)} call receive invalid packet.");
        }

        public async Task<bool> ClearError()
        {
            var responsePacket = await _communicationDriver.SendPacketAsync(new ArduinoPacket()
            {
                Command = ArduinoMediaSupplySystemCommands.ClearErrorCommand
            });

            if (responsePacket.Command == ArduinoMediaSupplySystemCommands.ClearErrorCommand)
            {
                return true;
            }

            throw new Exception($"{nameof(ClearError)} call receive invalid packet.");
        }

        #region Action Methods

        public async Task<bool> ReadyPump()
        {
            var responsePacket = await _communicationDriver.SendPacketAsync(new ArduinoPacket()
            {
                Command = ArduinoMediaSupplySystemCommands.MoveReadyPosCommand
            });

            if (responsePacket.Command == ArduinoMediaSupplySystemCommands.MoveReadyPosCommand)
            {
                return true;
            }

            throw new Exception($"{nameof(ReadyPump)} call receive invalid packet.");
        }

        public async Task<bool> StopPump()
        {
            var responsePacket = await _communicationDriver.SendPacketAsync(new ArduinoPacket()
            {
                Command = ArduinoMediaSupplySystemCommands.StopPumpCommand
            });

            if (responsePacket.Command == ArduinoMediaSupplySystemCommands.StopPumpCommand)
            {
                return true;
            }

            throw new Exception($"{nameof(StopPump)} call receive invalid packet.");
        }

        public async Task<bool> DispenseMedia(int pumpNumber, int volume)
        {
            var responsePacket = await _communicationDriver.SendPacketAsync(new ArduinoPacket()
            {
                Command = ArduinoMediaSupplySystemCommands.DispenseCommand,
                Arguments = new string[] { pumpNumber.ToString(), volume.ToString() }
            });

            if (responsePacket.Command == ArduinoMediaSupplySystemCommands.DispenseCommand)
            {
                return true;
            }

            throw new Exception($"{nameof(DispenseMedia)} call receive invalid packet.");
        }

        public async Task<bool> PrimeMedia(int pumpNumber)
        {
            var responsePacket = await _communicationDriver.SendPacketAsync(new ArduinoPacket()
            {
                Command = ArduinoMediaSupplySystemCommands.PrimeMediaCommand,
                Arguments = new string[] { pumpNumber.ToString() }
            });

            if (responsePacket.Command == ArduinoMediaSupplySystemCommands.DispenseCommand)
            {
                return true;
            }

            throw new Exception($"{nameof(PrimeMedia)} call receive invalid packet.");
        }

        public async Task<bool> PullMedia(int pumpNumber)
        {
            var responsePacket = await _communicationDriver.SendPacketAsync(new ArduinoPacket()
            {
                Command = ArduinoMediaSupplySystemCommands.PullMediaCommand,
                Arguments = new string[] { pumpNumber.ToString() }
            });

            if (responsePacket.Command == ArduinoMediaSupplySystemCommands.PullMediaCommand)
            {
                return true;
            }

            throw new Exception($"{nameof(PrimeMedia)} call receive invalid packet.");
        }
        #endregion

        public async Task<bool> MoveByPulse(int axisNumber, int pulseDistance)
        {
            var responsePacket = await _communicationDriver.SendPacketAsync(new ArduinoPacket()
            {
                Command = ArduinoMediaSupplySystemCommands.MoveAxisByPulseCommand,
                Arguments = new string[] { axisNumber.ToString(), pulseDistance.ToString() }
            });

            if (responsePacket.Command == ArduinoMediaSupplySystemCommands.MoveAxisByPulseCommand)
            {
                return true;
            }

            throw new Exception($"{nameof(MoveByPulse)} call receive invalid packet.");
        }

        public async Task<bool> MoveToPosition(int axisNumber, int pulsePosition)
        {
            var responsePacket = await _communicationDriver.SendPacketAsync(new ArduinoPacket()
            {
                Command = ArduinoMediaSupplySystemCommands.MoveAxisToPulseDistance,
                Arguments = new string[] { axisNumber.ToString(), pulsePosition.ToString() }
            });

            if (responsePacket.Command == ArduinoMediaSupplySystemCommands.MoveAxisToPulseDistance)
            {
                return true;
            }

            throw new Exception($"{nameof(MoveToPosition)} call receive invalid packet.");
        }

        public async Task<bool> SetAccelTime(int axisNumber, int accelMilliSeconds)
        {
            var responsePacket = await _communicationDriver.SendPacketAsync(new ArduinoPacket()
            {
                Command = ArduinoMediaSupplySystemCommands.SetAccelerateCommand,
                Arguments = new string[] { axisNumber.ToString(), accelMilliSeconds.ToString() }
            });

            if (responsePacket.Command == ArduinoMediaSupplySystemCommands.SetAccelerateCommand)
            {
                return true;
            }

            throw new Exception($"{nameof(SetAccelTime)} call receive invalid packet.");
        }

        public async Task<bool> SetSpeed(int axisNumber, int rpm)
        {
            var responsePacket = await _communicationDriver.SendPacketAsync(new ArduinoPacket()
            {
                Command = ArduinoMediaSupplySystemCommands.SetSpeedCommand,
                Arguments = new string[] { axisNumber.ToString(), rpm.ToString() }
            });

            if (responsePacket.Command == ArduinoMediaSupplySystemCommands.SetSpeedCommand)
            {
                return true;
            }

            throw new Exception($"{nameof(SetSpeed)} call receive invalid packet.");
        }
    }
}
