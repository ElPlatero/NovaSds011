using System;
using System.Linq;

namespace Codehaufen.Sds011
{
    internal class ParticularMatterDataPacket
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

        public override string ToString() => $"ID1: {Id1}, ID2: {Id2}, PM2.5: {Pm25Value:N1}, PM10: {Pm10Value:N1}";
    }
}