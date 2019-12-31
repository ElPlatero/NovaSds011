using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Codehaufen.Sds011
{
    internal class ParticularMatterDataReader
    {
        private readonly Stream _baseStream;

        public ParticularMatterDataReader(Stream baseStream)
        {
            _baseStream = baseStream;
        }

        public async Task<ICollection<ParticularMatterDataPacket>> ReadPacketsAsync(int bytesToRead)
        {
            var buffer = new byte[bytesToRead];
            var bytesRead = await _baseStream.ReadAsync(buffer, 0, buffer.Length);
            ICollection<ParticularMatterDataPacket> resultList = new List<ParticularMatterDataPacket>();

            for(var index = 0; index < bytesRead; index++)
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
                resultList.Add(result);
            }

            return resultList;
        }

        private static uint ReadUint(IReadOnlyList<byte> buffer, ref int index)
        {
            var lowByte = buffer[index++];
            var hiByte = buffer[index++];
            return (uint)((hiByte << 8) | lowByte);
        }
    }
}