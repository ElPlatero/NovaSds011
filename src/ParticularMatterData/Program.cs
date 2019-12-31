using System;
using System.IO.Ports;
using System.Linq;

namespace Codehaufen.Sds011
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] ports = SerialPort.GetPortNames();
            Console.WriteLine("The following serial ports were found:");
            foreach (string port in ports)
            {
                Console.WriteLine(port);
            }

            var portName = /* ports.First(p => p.Contains("USB", StringComparison.CurrentCultureIgnoreCase)) */ "COM3";

            using var sensor = new ParticularMatterSensor(portName);
            sensor.PacketReceived += (s, e) => Console.WriteLine(e);
            sensor.BeginReceive();
            while(true) { }
        }

    }
}