using FArduino;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MediaSupplyTestProgram
{
    class Program
    {
        static string HelpMessage = @"
connect
status
start
stop
dispense
exit
";
        static async Task Main(string[] args)
        {
            var ccwPulse = 20000;
            var cwPulse = -25000;
            var driver = new MediaSupplySystemDriver();
            var isRunningTest = false;
            var connection = driver.Connect("COM1", Convert.ToInt32(38400));
            
            if (connection) Console.WriteLine("Arduino is connected!");
            while (true)
            {
                Console.WriteLine("Please input:");
                var input = Console.ReadLine();
                switch (input) {
                    case "connect":
                        Console.Clear();
                        Console.WriteLine("COM port: ");
                        var comPort = Console.ReadLine();
                        Console.WriteLine("Buadrate: ");
                        var buadrate = Console.ReadLine();
                        if(driver.Connect(comPort, Convert.ToInt32(buadrate))){
                            Console.WriteLine("Connection Success");
                        }
                        else
                        {
                            Console.WriteLine("Connection Failure");
                        }
                        break;

                    case "status":
                        Console.Clear();
                        Console.WriteLine($"Connection : {driver.IsConnected()}");
                        // TODO: Show Media State.
                        break;
                    case "start":
                        if (!driver.IsConnected())
                        {
                            Console.WriteLine("Device is disconnected!");
                            break;
                        }

                        if(isRunningTest == true)
                        {
                            Console.WriteLine("Already Test is Running...");
                            break;
                        }

                        isRunningTest = true;
                        var task = Task.Run(async () => 
                        {
                            while(isRunningTest)
                            {
                                Console.WriteLine("Life Test Running... if you want to stop, Input 'stop'. ");
                                // TODO: Run Command
                                await Task.Delay(1000);
                                for(int pumpNumber = 1; pumpNumber < 5; pumpNumber++)
                                {
                                    
                                    await driver.MoveByPulse(pumpNumber, ccwPulse);
                                    var state = await driver.GetState();

                                    await Task.Delay(1000);
                                    while (state.mode == MediaSupplySystemMode.Move)
                                    {
                                        state = await driver.GetState();
                                        await Task.Delay(500);
                                    }
                                    await WriteLogAsync(pumpNumber, ccwPulse);

                                    await driver.MoveByPulse(pumpNumber, cwPulse);
                                    state = await driver.GetState();

                                    await Task.Delay(1000);
                                    while (state.mode == MediaSupplySystemMode.Move)
                                    {
                                        state = await driver.GetState();
                                        await Task.Delay(500);
                                    }
                                    await WriteLogAsync(pumpNumber, cwPulse);

                                    await Task.Delay(2000);
                                }
                            }
                            Console.WriteLine("Test Terminated!");
                        });
                        break;
                    case "stop":
                        isRunningTest = false;
                        await driver.StopPump();
                        await Task.Delay(1000);
                        var state = await driver.GetState();
                        while (state.mode == MediaSupplySystemMode.Move)
                        {
                            state = await driver.GetState();
                            await Task.Delay(500);
                        }
                        Console.WriteLine("Pump Stop");
                        break;
                    case "dispense":
                        Console.Clear();
                        Console.WriteLine("Channel (1~5) :");
                        int channel = Convert.ToInt32(Console.ReadLine());
                        Console.WriteLine("Volume (ml)    :");
                        int volume = Convert.ToInt32(Console.ReadLine());

                        // Prime
                        Console.WriteLine("Before Dispense, Priming...");
                        await driver.PrimeMedia(channel);
                        await Task.Delay(1000);
                        var status = await driver.GetState();
                        while (status.mode == MediaSupplySystemMode.Move)
                        {
                            await Task.Delay(1000);
                            status = await driver.GetState();
                        }
                        Console.WriteLine("Prime Finish!");
                        // Dispense
                        Console.WriteLine("Dispense Start!");
                        await driver.DispenseMedia(channel, volume);
                        await Task.Delay(1000);
                        status = await driver.GetState();
                        while (status.mode == MediaSupplySystemMode.Move)
                        {
                            await Task.Delay(1000);
                            status = await driver.GetState();
                        }
                        Console.WriteLine("Dispense Done!");
                        break;
                    case "exit":
                        await driver.StopPump();
                        Console.WriteLine("Terminated.");
                        return;
                    default:
                        Console.WriteLine($"{input} is wrong input!");
                        break;
                }
            }
        }

        static async Task WriteLogAsync(int pumpNumber, int pulse)
        {
            using StreamWriter file = new StreamWriter("action_log.csv", append: true);
            await file.WriteLineAsync($"{DateTime.Now.ToString()}, {pumpNumber}, {pulse}");
        }

     
    }
}
