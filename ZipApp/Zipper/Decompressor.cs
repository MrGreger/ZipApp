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
            while (true && !_cancelled)
            {
                var chunk = _transformationQueue.Dequeue();

                if (chunk == null)
                {
                    return;
                }

                if (chunk.ChunkOrder.Value == 5)
                {

                }

                if (chunk.Bytes[2] != 0)
                {
                    using (MemoryStream stream = new MemoryStream(chunk.Bytes))
                    {
                        using (GZipStream gzip = new GZipStream(stream, CompressionMode.Decompress))
                        {
                            var sizeBytes = new byte[] { chunk.Bytes[chunk.Bytes.Length - 4], chunk.Bytes[chunk.Bytes.Length - 3], chunk.Bytes[chunk.Bytes.Length - 2], chunk.Bytes[chunk.Bytes.Length - 1] };

                            var size = BitConverter.ToInt32(sizeBytes, 0);

                            if(size == 0)
                            {
                                size = _chunkSize;
                            }

                            byte[] buffer = new byte[size]; 

                            var bytesCount = gzip.Read(buffer, 0, size);

                            _writeQueue.Enqueue(new ByteChunk(buffer, chunk.ChunkOrder));
                        }
                    }
                }
                else
                {
                    var result = new byte[chunk.Bytes.Length - 8];

                    for (int i = 8, j = 0; i < chunk.Bytes.Length; i++, j++)
                    {
                        result[j] = chunk.Bytes[i];
                    }

                    _writeQueue.Enqueue(new ByteChunk(result, chunk.ChunkOrder));
                }
            }
        }

        protected override byte[] GetChunkBytes(Stream stream)
        {
            stream.Read(_chunkSizeBuffer, 0, _chunkHeaderSize);

            var chunkSize = BitConverter.ToInt32(_chunkSizeBuffer, 4);

            var buffer = new byte[chunkSize];

            stream.Read(buffer, _chunkHeaderSize, chunkSize - _chunkHeaderSize);

            _chunkSizeBuffer.CopyTo(buffer, 0);

            return buffer;
        }

    }
}