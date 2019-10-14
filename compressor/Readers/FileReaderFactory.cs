using Compressor.Interfaces;

namespace Compressor.Readers
{
    public class FileReaderFactory : IReaderFactory
    {
        private readonly string _path;

        public FileReaderFactory(string path)
        {
            _path = path;
        }

        public IFileReader CreateReader()
        {
            return new FileDataReader(_path);
        }
    }
}
