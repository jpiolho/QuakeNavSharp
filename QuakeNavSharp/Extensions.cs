using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QuakeNavSharp
{
    static class Extensions
    {
        public static Vector3 ReadVector3(this BinaryReader reader)
        {
            return new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }

        public static void Write(this BinaryWriter writer, Vector3 vector)
        {
            writer.Write((float)vector.X);
            writer.Write((float)vector.Y);
            writer.Write((float)vector.Z);
        }


        public static async Task ReadAsync(this Stream stream,Stream targetStream,int count,bool positionZero,CancellationToken cancellationToken=default)
        {
            if (positionZero)
                targetStream.Position = 0;

            try
            {
                await stream.ReadAsync(targetStream, count, cancellationToken);
            }
            finally
            {
                if(positionZero)
                    targetStream.Position = 0;
            }

        }

        public static async Task ReadAsync(this Stream stream, Stream targetStream,int count,CancellationToken cancellationToken=default)
        {
            const int bufferLength = 1024 * 32; // 32kb
            var pool = ArrayPool<byte>.Shared;
            var buffer = pool.Rent(bufferLength);


            try
            {
                var bytesLeft = count;

                while (bytesLeft > 0) {
                    var requestedRead = Math.Min(bufferLength, bytesLeft); // Calculate how much to read
                    var actualRead = await stream.ReadAsync(buffer, 0, requestedRead,cancellationToken);

                    // Throw exception if the requested amount was not available to be read
                    if (actualRead < requestedRead)
                        throw new InvalidDataException($"Unexpected end of stream");

                    // Write to the target stream
                    await targetStream.WriteAsync(buffer, 0, actualRead,cancellationToken);

                    bytesLeft -= actualRead; // Decrement the amount of bytes left to read
                }
            }
            finally
            {
                pool.Return(buffer);
            }
        }
    }
}
