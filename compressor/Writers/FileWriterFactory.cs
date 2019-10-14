using Compressor.Interfaces;

namespace Compressor.Writers
{
    public class FileWriterFactory : IWriterFactory
    {
        private readonly string _path;

        public FileWriterFactory(string path)
        {
            _path = path;
        }

        public IFileWriter CreateWriter()
        {
            return new FileDataWriter(_path);
        }
    }
}
