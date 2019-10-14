using System.IO;
using System.IO.Compression;
using System.Threading;
using Compressor.Auxiliary;
using Compressor.Dispatchers;
using Compressor.Interfaces;

namespace Compressor.Compression
{
    public class CompressDataDispatcher : DataDispatcher
    {
        private const int _chunkSize = 1024 * 1024;
        private long _readOffset;
        private long _writeOffset;

        public CompressDataDispatcher(IReaderFactory readerFactory, IWriterFactory writerFactory, int readThreadsCount, int writeThreadsCount)
            : base(readerFactory, writerFactory, readThreadsCount, writeThreadsCount)
        {
        }

        protected override DataChunk Read(IFileReader dataReader)
        {
            var offset = Interlocked.Add(ref _readOffset, _chunkSize) - _chunkSize;
            DataChunk dataChunk = null;

            if (offset < dataReader.Size)
            {
                var data = dataReader.ReadChunk(offset, _chunkSize);
                dataChunk = new DataChunk(offset, data);
            }

            return dataChunk;
        }

        protected override void Write(IFileWriter dataWriter, DataChunk chunk)
        {
            var prefixSize = StructSerializer.GetSize<GzipChunkPrefix>();

            using (MemoryStream memoryStream = new MemoryStream(prefixSize + chunk.Data.Length))
            {
                using (GZipStream compressionStream = new GZipStream(memoryStream, CompressionMode.Compress, leaveOpen: true))
                {
                    memoryStream.Position = prefixSize;
                    compressionStream.Write(chunk.Data, 0, chunk.Data.Length);
                }

                var prefix = new GzipChunkPrefix()
                {
                    ChunkSize = chunk.Data.Length,
                    Offset = chunk.Offset,
                    GzipDataSize = (int)memoryStream.Length - prefixSize
                };

                memoryStream.Position = 0;
                var prefixBytes = StructSerializer.Serialize(prefix);
                memoryStream.Write(prefixBytes, 0, prefixBytes.Length);

                memoryStream.Capacity = (int)memoryStream.Length;
                var compressedData = memoryStream.GetBuffer();

                var offset = Interlocked.Add(ref _writeOffset, compressedData.Length) - compressedData.Length;
                dataWriter.WriteChunk(offset, compressedData);
            }
        }
    }
}
