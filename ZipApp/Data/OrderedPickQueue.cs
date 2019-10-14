using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZipApp.Data
{
    public class OrderedPickQueue 
    {
        protected bool _closed;

        protected Dictionary<int,ByteChunk> _chunks;
        protected int _chunkCounter = 0;

        protected object _queueLocker = new object();

        public OrderedPickQueue()
        {
            _chunks = new Dictionary<int, ByteChunk>();
        }

        public void Enqueue(ByteChunk chunk)
        {
            lock (_queueLocker)
            {
                if (_closed)
                {
                    throw new InvalidOperationException("Queue is closed");
                }

                if(chunk.ChunkOrder.HasValue == false)
                {
                    throw new InvalidOperationException("Chunk must have order");
                }

                _chunks.Add(chunk.ChunkOrder.Value,chunk);

            Monitor.PulseAll(_queueLocker);
        }
    }

        public ByteChunk Dequeue()
        {
            lock (_queueLocker)
            {
                while (_chunks.Count == 0 && !_closed)
                {
                    Monitor.Wait(_queueLocker);
                }

                if (_chunks.Count > 0)
                {
                    while (!_chunks.ContainsKey(_chunkCounter))
                    {
                        Monitor.Wait(_queueLocker);
                    }                    
  
                    var result = _chunks[_chunkCounter];
                    _chunks.Remove(_chunkCounter);

                    _chunkCounter++;

                    Monitor.PulseAll(_queueLocker);

                    return result;
                }

                return null;
            }
        }

        public void Close()
        {
            lock (_queueLocker)
            {
                _closed = true;
                Monitor.PulseAll(_queueLocker);
            }
        }
    }
}