using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZipApp.Data
{
    public class ByteChunk
    {
        private int? _chunkOrder;

        public int? ChunkOrder { get => _chunkOrder; set { if (_chunkOrder != null) return; _chunkOrder = value; } }
        public byte[] Bytes { get; }

        public ByteChunk(byte[] bytes, int? chunkOrder = null)
        {
            Bytes = bytes;
            ChunkOrder = chunkOrder;
        }

    }
}
