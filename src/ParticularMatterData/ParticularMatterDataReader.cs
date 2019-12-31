using System.Collections.Generic;
using System.IO;

namespace Codehaufen.Sds011
{
    internal class ParticularMatterDataReader
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
            for(var index = 0; index < buffer.Length; index++)
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

        private static uint ReadUint(IReadOnlyList<byte> buffer, ref int index)
        {
            var lowByte = buffer[index++];
            var hiByte = buffer[index++];
            return (uint)((hiByte << 8) | lowByte);
        }
    }
}