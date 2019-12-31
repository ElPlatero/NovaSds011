using System;
using System.IO.Ports;
using System.Threading.Tasks;

namespace Codehaufen.Sds011
{
    internal class ParticularMatterSensor : IDisposable
    {
        private readonly SerialPort _port;

        public ParticularMatterSensor(string portName)
        {
            _port = new SerialPort(portName, 9600, Parity.None, 8);
            _port.DataReceived += async (s, e) => await PortOnDataReceivedAsync(s, e);
        }

        public void BeginReceive() => _port.Open();
        public void Dispose() => _port?.Dispose();

        private async Task PortOnDataReceivedAsync(object sender, SerialDataReceivedEventArgs e)
        {
            if (e.EventType != SerialData.Chars || !(sender is SerialPort serialPort)) return;
            
            var bufferSize = serialPort.BytesToRead;
            var reader = new ParticularMatterDataReader(serialPort.BaseStream);
            var resultList = await reader.ReadPacketsAsync(bufferSize);
            foreach (var packet in resultList)
            {
                OnPacketReceived(packet);
            }
        }

        public event EventHandler<ParticularMatterDataPacket> PacketReceived;
        private void OnPacketReceived(ParticularMatterDataPacket packet) => PacketReceived?.Invoke(this, packet);

    }
}