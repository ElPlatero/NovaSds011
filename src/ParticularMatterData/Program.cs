using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks.Dataflow;

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

            var portName = ports.First(p => p.Contains("USB", StringComparison.CurrentCultureIgnoreCase));

            Console.WriteLine($"using port {portName}...");
            using (var port = new SerialPort(portName, 9600, Parity.None, 8))
            {
                port.DataReceived += PortOnDataReceived;

                port.Open();
                while (true)
                {
                }
            }
        }

        private static void PortOnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (e.EventType == SerialData.Chars && sender is SerialPort serialPort)
            {
                var bufferSize = serialPort.BytesToRead;
                var reader = new ParticularMatterDataReader(serialPort.BaseStream);
                var resultList = reader.ReadPackets(bufferSize).ToList();
                foreach (var result in resultList)
                {
                    Console.WriteLine($"ID1: {result.Id1}, ID2: {result.Id2}, PM2.5: {result.Pm25Value:N1}, PM10: {result.Pm10Value:N1}");
                    Console.WriteLine();
                }
            }
        }
    }

    class ParticularMatterDataReader
    {
        private readonly Stream _baseStream;

        public ParticularMatterDataReader(Stream baseStream)
        {
            _baseStream = baseStream;
        }

        public IEnumerable<ParticularMatterDataPacket> ReadPackets(int bytesToRead)
        {
            var buffer = new byte[bytesToRead];
            _baseStream.Read(buffer, 0, buffer.Length);
            for(int index = 0; index < buffer.Length; index++)
            {
                if (buffer[index] != ParticularMatterDataPacket.MessageHeader) continue;
                if (buffer[++index] != ParticularMatterDataPacket.CommanderNo) continue;

                index++;
                var result = new ParticularMatterDataPacket();
                result.SetPm25(ReadUint(buffer, ref index));
                result.SetPm10(ReadUint(buffer, ref index));

                result.Id1 = buffer[index++];
                result.Id2 = buffer[index++];

                if (buffer[index++] != result.Checksum) continue;
                if (buffer[index] != ParticularMatterDataPacket.MessageTail) continue;
                yield return result;
            }
        }

        private uint ReadUint(byte[] buffer, ref int index)
        {
            var lowByte = buffer[index++];
            var hiByte = buffer[index++];
            return (uint)((hiByte << 8) | lowByte);
        }
    }

    class ParticularMatterDataPacket
    {
        public const byte MessageHeader = 0xAA;
        public const byte CommanderNo = 0xC0;
        public void SetPm10(uint rawValue) => Pm10Value = rawValue / 10m;
        public void SetPm25(uint rawValue) => Pm25Value = rawValue / 10m;
        public decimal Pm25Value { get; private set; }
        public decimal Pm10Value { get; private set; }
        public byte Id1 { get; set; }
        public byte Id2 { get; set; }

        public byte Checksum
        {
            get
            {
                var pm25Bytes = BitConverter.GetBytes((uint)(Pm25Value * 10u));
                var pm10Bytes = BitConverter.GetBytes((uint)(Pm10Value * 10u));
                return BitConverter.GetBytes(pm10Bytes[0] + pm10Bytes[1] + pm25Bytes[0] + pm25Bytes[1] + Id1 + Id2).First();
            }
        }
        public const byte MessageTail = 0xAB;
    }
}