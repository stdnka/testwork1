using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Compressor.Auxiliary
{
    public class ConcurrentBuffer<TValue>
    {
        private readonly int _size;
        private readonly Queue<TValue> _buffer;
        private readonly object _locker;
        private bool _stopped;

        public ConcurrentBuffer(int size)
        {
            _size = size;
            _buffer = new Queue<TValue>(size);
            _locker = new object();
        }

        public bool Put(TValue item)
        {
            lock (_locker)
            {
                while (_buffer.Count == _size && !_stopped)
                {
                    Monitor.Wait(_locker);
                }

                if (_stopped)
                {
                    Console.WriteLine($"Put: buffer size = {_buffer.Count}, stopped = {_stopped}");
                    return false;
                }

                _buffer.Enqueue(item);
                Console.WriteLine($"Put: buffer size = {_buffer.Count}, stopped = {_stopped}");

                Monitor.PulseAll(_locker);
            }

            return true;
        }

        public bool Get(out TValue item)
        {
            item = default(TValue);

            lock (_locker)
            {
                while (!_buffer.Any() && !_stopped)
                {
                    Monitor.Wait(_locker);
                }

                if (!_buffer.Any() && _stopped)
                {
                    Console.WriteLine($"Get: buffer size = {_buffer.Count}, stopped = {_stopped}");
                    return false;
                }

                item = _buffer.Dequeue();
                Console.WriteLine($"Get: buffer size = {_buffer.Count}, stopped = {_stopped}");

                Monitor.PulseAll(_locker);
            }

            return true;
        }

        public void Stop()
        {
            lock (_locker)
            {
                _stopped = true;
                Monitor.PulseAll(_locker);
            }
        }
    }
}
