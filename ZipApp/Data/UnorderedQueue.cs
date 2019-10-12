using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZipApp.Data
{
    public class UnorderedQueue : ConcurentQueue
    {
        private int maxObjects = 10;

        public override void Enqueue(ByteChunk chunk)
        {
            lock (_queueLocker)
            {
                if (_closed)
                {
                    throw new InvalidOperationException("Queue is closed");
                }

                if (!chunk.ChunkOrder.HasValue)
                {
                    chunk.ChunkOrder = _chunkCounter;
                }

                while (_chunks.Count >= maxObjects)
                    Monitor.Wait(_queueLocker);

                _chunks.Enqueue(chunk);
                _chunkCounter++;
                Monitor.PulseAll(_queueLocker);
            }
        }
    }
}
