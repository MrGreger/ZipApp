using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZipApp.Data
{
    public class OrderedPushQueue 
    {
        private int _maxObjects;

        protected bool _closed;

        protected Queue<ByteChunk> _chunks;
        protected int _chunkCounter = 0;

        protected object _queueLocker = new object();

        public OrderedPushQueue(int maxObjectsInQueue)
        {
            _maxObjects = maxObjectsInQueue;
            _chunks = new Queue<ByteChunk>();
        }

        public bool QueueIsFull()
        {
            lock (_queueLocker)
            {
                return _chunks.Count == _maxObjects;
            }
        }

        public void Enqueue(ByteChunk chunk)
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

                while (_chunks.Count >= _maxObjects)
                    Monitor.Wait(_queueLocker);

                _chunks.Enqueue(chunk);
                _chunkCounter++;
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
                    var result = _chunks.Dequeue();
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
