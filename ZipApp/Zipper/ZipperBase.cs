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
        protected int _chunkCrcBlockLength = 4;
        protected OrderedPushQueue _transformationQueue;
        protected OrderedPickQueue _writeQueue;

        private ManualResetEvent[] _onTransformationThreadEnd;

        public ZipperBase(string filePath, string resultPath)
        {
            _filePath = filePath;
            _resultPath = resultPath;
            _transformationQueue = new OrderedPushQueue(_threadsForTransformationCount * 2);
            _writeQueue = new OrderedPickQueue();
            _onTransformationThreadEnd = new ManualResetEvent[_threadsForTransformationCount];
        }

        public void Start()
        {
            try
            {
                if (!CanMultithreadZip())
                {
                    Console.WriteLine("Processor cores count not enough for multithread zipping or unzipping.");
                    _cancelled = true;
                    return;
                }

                new Thread(ReadFile).Start();

                while (_transformationQueue.QueueIsFull() == false && !_cancelled)
                {
                    //wait 
                }

                for (int i = 0; i < _threadsForTransformationCount; i++)
                {
                    _onTransformationThreadEnd[i] = new ManualResetEvent(false);
                    var thread = new Thread(new ParameterizedThreadStart(TransformFile));
                    thread.Start(i);
                }

                var writeQueue = new Thread(WriteQueue);

                writeQueue.Start();

                WaitHandle.WaitAll(_onTransformationThreadEnd);

                _writeQueue.Close();

                writeQueue.Join();

                _succeeded = true;
            }
            finally
            {
                CloseEvents();
            }
        }

        private bool CanMultithreadZip()
        {
            return _threadsForTransformationCount > 1;
        }

        private void CloseEvents()
        {
            for (int i = 0; i < _onTransformationThreadEnd.Length; i++)
            {
                _onTransformationThreadEnd[i]?.Close();
            }
        }

        public void Stop()
        {
            CancelWork();
        }

        private void CancelWork()
        {
            _cancelled = true;
            _writeQueue.Close();
            _transformationQueue.Close();
            StopAllTransformationThreads();
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
                CancelWork();
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

                CancelWork();
            }
        }

        private void StopAllTransformationThreads()
        {
            for (int i = 0; i < _onTransformationThreadEnd.Length; i++)
            {
                if(_onTransformationThreadEnd[i] != null && _onTransformationThreadEnd[i].SafeWaitHandle.IsClosed == false) { 
                _onTransformationThreadEnd[i]?.Set();}
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
                CancelWork();
            }
        }

    }
}