namespace Compressor.Dispatchers
{
    public class DataChunk
    {
        public DataChunk(long offset, byte[] data)
        {
            Offset = offset;
            Data = data;
        }

        public long Offset { get; }

        public byte[] Data { get; }
    }
}
