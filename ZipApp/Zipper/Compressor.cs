using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZipApp.Data;

namespace ZipApp.Zipper
{
    public class Compressor : ZipperBase
    {
        public Compressor(string filePath, string resultPath) : base(filePath, resultPath)
        {

        }

        protected override void Transform()
        {
            while (true && !_cancelled)
            {
                var chunk = _transformationQueue.Dequeue();

                if (chunk == null)
                {
                    break;
                }

                using (MemoryStream stream = new MemoryStream())
                {
                    using (GZipStream gzip = new GZipStream(stream, CompressionMode.Compress))
                    {
                        gzip.Write(chunk.Bytes, 0, chunk.Bytes.Length);
                    }

                    byte[] buffer = stream.ToArray();

                    if (chunk.Bytes.Length + 8 < buffer.Length)
                    {
                        byte[] newBuffer = new byte[chunk.Bytes.Length + 8];

                        BitConverter.GetBytes(newBuffer.Length).CopyTo(newBuffer, 4);

                        chunk.Bytes.CopyTo(newBuffer, _chunkHeaderSize);

                        _writeQueue.Enqueue(new ByteChunk(newBuffer, chunk.ChunkOrder));
                    }
                    else
                    {
                        BitConverter.GetBytes(buffer.Length).CopyTo(buffer, _chunkSizeBytesCount);

                        var result = new ByteChunk(buffer, chunk.ChunkOrder);

                        _writeQueue.Enqueue(result);
                    }
                }
            }
        }

        protected override byte[] GetChunkBytes(Stream stream)
        {

            var _buffer = new byte[_chunkSize];

            var remainingBytes = stream.Length - stream.Position;

            var bufferSize = remainingBytes >= _chunkSize ? _chunkSize : remainingBytes;

            stream.Read(_buffer, 0, (int)bufferSize);

            if (bufferSize < _chunkSize)
            {
                var smallBuffer = new byte[bufferSize];

                for (int i = 0; i < bufferSize; i++)
                {
                    smallBuffer[i] = _buffer[i];
                }

                return smallBuffer;
            }

            return _buffer;
        }
    }
}