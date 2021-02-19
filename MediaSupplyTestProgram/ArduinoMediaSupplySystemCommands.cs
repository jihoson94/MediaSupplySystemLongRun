namespace MediaSupplyTestProgram
{
    // Arduino Media Supply System Reference 
    // https://docs.google.com/document/d/1ipe2x-acrJpKfkE7iiNnWHcH8641m9AwMTmkD1p4yC0/edit
    public class ArduinoMediaSupplySystemCommands
    {
        public const string GetErrorCodeCommand = "GERR"; // Get Error Code.
        public const string GetErrorDescriptionCommand = "GERD"; // Get Error Description( with whitespace)
        public const string ClearErrorCommand = "CLER"; // Clear Error

        public const string GetStatusCommand = "GSTA";
        public const string MoveReadyPosCommand = "REDY";
        public const string StopPumpCommand = "STOP";

        public const string SetSpeedCommand = "SSPE"; // SSPE [pump #], [RPM]
        public const string SetAccelerateCommand = "SACC"; // SACC [pump #], [acceleration]

        public const string MoveAxisByPulseCommand = "MOVB"; // MOVB [Pump #],[Pulse Distance]
        public const string MoveAxisToPulseDistance = "MOVT"; // MOVT [Pump #],[Pulse Position]
        public const string PrimeMediaCommand = "PRIM"; // PRIM [Pump #]
        public const string DispenseCommand = "DISP"; // DISP [Pump #] [dispensing volume (ml)]
        public const string PullMediaCommand = "PULL"; // DISP [Pump #] [dispensing volume (ml)]
    }
}
