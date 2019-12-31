using System;
using System.IO.Ports;
#if !DEBUG
using System.Linq;
#endif

namespace Codehaufen.Sds011
{
    internal class Program
    {
        private static void Main()
        {
            var ports = SerialPort.GetPortNames();
            Console.WriteLine("The following serial ports were found:");
            foreach (var port in ports)
            {
                Console.WriteLine(port);
            }
            #if DEBUG
            const string portName = "COM3";
            #else
            var portName = ports.First(p => p.Contains("USB", StringComparison.CurrentCultureIgnoreCase));
            #endif

            using var sensor = new ParticularMatterSensor(portName);
            sensor.PacketReceived += (s, e) => Console.WriteLine(e);
            sensor.BeginReceive();
            while(true) { }
        }

    }
}