namespace Compressor.Interfaces
{
    public interface IFileReader
    {
        long Size { get; }

        byte[] ReadChunk(long offset, int size);
    }
}
