using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZipApp
{
    public class BuffersPool
    {
        private static object locker = new object();

        private static List<byte[]> _free = new List<byte[]>();
        private static List<byte[]> _notFree = new List<byte[]>();

        public static byte[] GetBuffer(int size)
        {
            lock (locker)
            {
                var result = _free.FirstOrDefault(x => x.Length == size);

                if (result == null)
                {
                    for (int i = 0; i < 100; i++)
                    {
                        var newArray = new byte[size];
                        _free.Add(newArray);
                    }

                    result = _free.FirstOrDefault(x => x.Length == size);

                    _free.Remove(result);
                    _notFree.Add(result);
                    return result;
                }

                _free.Remove(result);
                _notFree.Add(result);

                return result;
            }
        }

        public static void Release(byte[] array)
        {
            lock (locker)
            {
                _notFree.Remove(array);
                _free.Add(array);
            }
        }
    }
}
