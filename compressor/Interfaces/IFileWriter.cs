namespace Compressor.Interfaces
{
    public interface IFileWriter
    {
        void WriteChunk(long offset, byte[] data);
    }
}
