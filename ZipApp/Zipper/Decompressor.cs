using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZipApp.Data;

namespace ZipApp.Zipper
{
    public class Decompressor : ZipperBase
    {
        private byte[] _chunkSizeBuffer;

        public Decompressor(string filePath, string resultPath) : base(filePath, resultPath)
        {
            _chunkSizeBuffer = new byte[_chunkHeaderSize];
        }

        protected override void Transform()
        {
            byte[] buffer = new byte[_chunkSize]; ;

            while (true && !_cancelled)
            {
                var chunk = _transformationQueue.Dequeue();

                if (chunk == null)
                {
                    return;
                }

                using (MemoryStream stream = new MemoryStream(chunk.Bytes))
                {
                    using (GZipStream gzip = new GZipStream(stream, CompressionMode.Decompress))
                    {
                        var bytesCount = gzip.Read(buffer, 0, buffer.Length);

                        var result = new byte[bytesCount];

                        for (int i = 0; i < bytesCount; i++)
                        {
                            result[i] = buffer[i];
                        }

                        _writeQueue.Enqueue(new ByteChunk(result, chunk.ChunkOrder));
                    }
                }

            }
        }

        protected override byte[] GetChunkBytes(Stream stream)
        {
            stream.Read(_chunkSizeBuffer, 0, _chunkHeaderSize);

            var chunkSize = BitConverter.ToInt32(_chunkSizeBuffer, 4);

            var buffer = new byte[chunkSize + _chunkHeaderSize];

            stream.Read(buffer, _chunkHeaderSize, chunkSize - _chunkHeaderSize);

            _chunkSizeBuffer.CopyTo(buffer, 0);

            return buffer;
        }

    }
}