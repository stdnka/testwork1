using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using Compressor.Auxiliary;
using Compressor.Dispatchers;
using Compressor.Interfaces;

namespace Compressor.Compression
{
    public class DecompressDataDispatcher : DataDispatcher
    {
        private long _readOffset;
        private long _bytesWritten;
        private readonly object _locker;

        public DecompressDataDispatcher(IReaderFactory readerFactory, IWriterFactory writerFactory, int readThreadsCount, int writeThreadsCount)
            : base(readerFactory, writerFactory, readThreadsCount, writeThreadsCount)
        {
            _locker = new object();
        }

        protected override DataChunk Read(IFileReader dataReader)
        {
            long offset;
            GzipChunkPrefix prefix;
            DataChunk dataChunk = null;

            lock (_locker)
            {
                if (_readOffset >= dataReader.Size)
                    return null;

                var prefixSize = StructSerializer.GetSize<GzipChunkPrefix>();
                var prefixBytes = dataReader.ReadChunk(_readOffset, prefixSize);
                prefix = StructSerializer.Deserialize<GzipChunkPrefix>(prefixBytes);
                offset = _readOffset + prefixSize;
                _readOffset = offset + prefix.GzipDataSize;
            }

            
            var decompressedData = new byte[prefix.ChunkSize];
            var data = dataReader.ReadChunk(offset, prefix.GzipDataSize);

            using (MemoryStream memoryStream = new MemoryStream(data))
            {
                using (GZipStream decompressionStream = new GZipStream(memoryStream, CompressionMode.Decompress, leaveOpen: true))
                {
                    var decompressedDataSize = decompressionStream.Read(decompressedData, 0, prefix.ChunkSize);

                    if (decompressedDataSize != prefix.ChunkSize)
                    {
                        Console.WriteLine("The specified compressed file is malformed.");
                    }
                }

                dataChunk = new DataChunk(prefix.Offset, decompressedData);
            }

            return dataChunk;
        }

        protected override void Write(IFileWriter dataWriter, DataChunk chunk)
        {
            dataWriter.WriteChunk(chunk.Offset, chunk.Data);
            Interlocked.Add(ref _bytesWritten, chunk.Data.Length);
        }
    }
}
