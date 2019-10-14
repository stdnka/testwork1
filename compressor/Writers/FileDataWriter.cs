using System.IO;
using Compressor.Interfaces;

namespace Compressor.Writers
{
    public class FileDataWriter : IFileWriter
    {
        private readonly FileStream _fileStream;

        public FileDataWriter(string path)
        {
            _fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Write);
        }

        public void WriteChunk(long offset, byte[] data)
        {
            _fileStream.Position = offset;
            _fileStream.Write(data, 0, data.Length);
            _fileStream.Flush();
        }
    }
}
