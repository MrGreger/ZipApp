using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZipApp.Data
{
    public abstract class ConcurentQueue
    {
        protected bool _closed;

        protected Queue<ByteChunk> _chunks;
        protected int _chunkCounter = 0;

        protected object _queueLocker = new object();

        public ConcurentQueue()
        {
            _chunks = new Queue<ByteChunk>();
        }

        public abstract void Enqueue(ByteChunk chunk);  

        public ByteChunk Dequeue()
        {
            lock (_queueLocker)
            {
                while (_chunks.Count == 0 && !_closed)
                {
                    Monitor.Wait(_queueLocker);
                }

                if(_chunks.Count > 0)
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
