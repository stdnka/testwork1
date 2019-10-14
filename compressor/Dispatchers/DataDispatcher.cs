using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Compressor.Auxiliary;
using Compressor.Interfaces;

namespace Compressor.Dispatchers
{
    public abstract class DataDispatcher : IDataDispatcher
    {
        private readonly IReaderFactory _readerFactory;
        private readonly IWriterFactory _writerFactory;
        private readonly ConcurrentBuffer<DataChunk> _chunkQueue;
        
        private readonly int _readThreadsCount;
        private readonly int _writeThreadsCount;
        private readonly Exception[] _exceptions;
        private List<Thread> _readThreads;
        private List<Thread> _writeThreads;

        protected DataDispatcher(IReaderFactory readerFactory, IWriterFactory writerFactory, int readThreadsCount, int writeThreadsCount)
        {
            _readerFactory = readerFactory;
            _writerFactory = writerFactory;
            _readThreadsCount = readThreadsCount;
            _writeThreadsCount = writeThreadsCount;
            _exceptions = new Exception[_readThreadsCount + _writeThreadsCount];
            _chunkQueue = new ConcurrentBuffer<DataChunk>(8);
        }

        public void Start()
        {
            _readThreads = new List<Thread>();
            _writeThreads = new List<Thread>();

            try
            {
                for (int i = 0; i < _readThreadsCount; ++i)
                {
                    var ilocal = i;
                    var reader = _readerFactory.CreateReader();
                    var thread = new Thread(() => ReaderProc(reader, ref _exceptions[ilocal]));
                    thread.Start();
                    _readThreads.Add(thread);
                }

                for (int i = 0; i < _writeThreadsCount; ++i)
                {
                    var ilocal = i;
                    var writer = _writerFactory.CreateWriter();
                    var thread = new Thread(() => WriterProc(writer, ref _exceptions[_readThreadsCount + ilocal]));
                    thread.Start();
                    _writeThreads.Add(thread);
                }
            }
            catch (Exception ex)
            {
                _chunkQueue.Stop();
                WaitForCompletion();
                throw;
            }
        }

        public Exception WaitForCompletion()
        {
            foreach (var thread in _readThreads)
            {
                thread.Join();
            }

            _chunkQueue.Stop();

            foreach (var thread in _writeThreads)
            {
                thread.Join();
            }

            var exceptions = _exceptions.Where(ex => ex != null).ToArray();

            if (exceptions.Any())
            {
                return new AggregateException("Operation failed.", exceptions);
            }

            return null;
        }

        protected abstract DataChunk Read(IFileReader dataReader);

        protected abstract void Write(IFileWriter dataWriter, DataChunk chunk);

        private void ReaderProc(IFileReader dataReader, ref Exception ex)
        {
            try
            {
                bool proceed = true;

                while (proceed)
                {
                    var chunk = Read(dataReader);

                    if (chunk == null)
                        break;

                    Console.WriteLine($"A chunk was read at offset {chunk.Offset}");
                    proceed = _chunkQueue.Put(chunk);
                }
            }
            catch (Exception exception)
            {
                _chunkQueue.Stop();
                ex = exception;
            }
        }

        private void WriterProc(IFileWriter dataWriter, ref Exception ex)
        {
            try
            {
                bool proceed = _chunkQueue.Get(out var chunk);

                while (proceed)
                {
                    Write(dataWriter, chunk);
                    Console.WriteLine($"A chunk was written at offset {chunk.Offset}");
                    proceed = _chunkQueue.Get(out chunk);
                }
            }
            catch (Exception exception)
            {
                _chunkQueue.Stop();
                ex = exception;
            }
        }
    }
}
