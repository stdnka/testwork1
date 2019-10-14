using System;
using System.IO;
using Compressor.Interfaces;

namespace Compressor.Readers
{
    public class FileDataReader : IFileReader
    {
        private readonly FileStream _fileStream;
        public FileDataReader(string path)
        {
            _fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        public long Size => _fileStream.Length;

        public byte[] ReadChunk(long offset, int size)
        {
            int dataSize = (int)(Math.Min(offset + size, Size) - offset);
            var data = new byte[dataSize];

            _fileStream.Position = offset;
            var bytesRead = _fileStream.Read(data, 0, dataSize);

            if (bytesRead != dataSize)
            {
                Array.Resize(ref data, bytesRead);
            }

            return data;
        }
    }
}
