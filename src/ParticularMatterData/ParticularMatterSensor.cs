using System;
using System.IO.Ports;
using System.Linq;

namespace Codehaufen.Sds011
{
    internal class ParticularMatterSensor : IDisposable
    {
        private readonly SerialPort _port;

        public ParticularMatterSensor(string portName)
        {
            _port = new SerialPort(portName, 9600, Parity.None, 8);
            _port.DataReceived += PortOnDataReceived;
        }

        public void BeginReceive() => _port.Open();
        public void Dispose() => _port?.Dispose();

        private void PortOnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (e.EventType != SerialData.Chars || !(sender is SerialPort serialPort)) return;
            
            var bufferSize = serialPort.BytesToRead;
            var reader = new ParticularMatterDataReader(serialPort.BaseStream);
            var resultList = reader.ReadPackets(bufferSize).ToList();
            resultList.ForEach(OnPacketReceived);
        }

        public event EventHandler<ParticularMatterDataPacket> PacketReceived;
        private void OnPacketReceived(ParticularMatterDataPacket packet) => PacketReceived?.Invoke(this, packet);

    }
}