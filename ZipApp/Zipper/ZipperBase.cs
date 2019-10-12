using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZipApp.Data;

namespace ZipApp.Zipper
{
    public abstract class ZipperBase
    {
        public int ResultCode => (_succeeded && !_cancelled) ? 0 : 1;

        private int _threadsForTransformationCount = Environment.ProcessorCount - 2;
        private bool _succeeded = false;
        protected bool _cancelled = false;

        private string _filePath;
        private string _resultPath;

        protected int _chunkSize = 1024 * 1024;
        protected int _chunkHeaderSize = 8;
        protected int _chunkSizeBytesCount = 4;
        protected ConcurentQueue _transformationQueue;
        protected ConcurentQueue _writeQueue;

        private ManualResetEvent[] _onTransformationThreadEnd;

        public ZipperBase(string filePath, string resultPath)
        {
            _filePath = filePath;
            _resultPath = resultPath;
            _transformationQueue = new UnorderedQueue();
            _writeQueue = new OrderedQueue();
            _onTransformationThreadEnd = new ManualResetEvent[_threadsForTransformationCount];
        }

        public void Start()
        {
            new Thread(ReadFile).Start();

            for (int i = 0; i < _threadsForTransformationCount; i++)
            {
                _onTransformationThreadEnd[i] = new ManualResetEvent(false);
                new Thread(new ParameterizedThreadStart(TransformFile)).Start(i);
            }

            new Thread(WriteQueue).Start();

            WaitHandle.WaitAll(_onTransformationThreadEnd);

            _succeeded = true;
        }

        public void Stop()
        {
            _cancelled = true;
        }

        protected abstract void Transform();
        protected abstract byte[] GetChunkBytes(Stream stream);

        private void ReadFile()
        {
            try
            {
                using (FileStream fs = new FileStream(_filePath, FileMode.Open))
                {
                    byte[] buffer;

                    while (fs.Position != fs.Length && !_cancelled)
                    {
                        buffer = GetChunkBytes(fs);

                        _transformationQueue.Enqueue(new ByteChunk(buffer));
                    }

                    _transformationQueue.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error :" + e.Message);
                _cancelled = true;
            }
          
        }

        private void TransformFile(object state)
        {
            try
            {
                Transform();
                OnTransformationThreadEnd((int)state);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error :" + e.Message);
                _cancelled = true;
            }
        }

        protected void OnTransformationThreadEnd(int id)
        {
            _onTransformationThreadEnd[id].Set();
        }

        private void WriteQueue()
        {
            try
            {
                using (FileStream fs = new FileStream(_resultPath, FileMode.OpenOrCreate))
                {
                    while (true && !_cancelled)
                    {
                        var chunk = _writeQueue.Dequeue();

                        if (chunk == null)
                        {
                            break;
                        }

                        fs.Write(chunk.Bytes, 0, chunk.Bytes.Length);
                        fs.Flush();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error :" + e.Message);
                _cancelled = true;
            }
        }

    }
}