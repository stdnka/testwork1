using System.Runtime.InteropServices;

namespace Compressor.Dispatchers
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct GzipChunkPrefix
    {
        public int GzipDataSize { get; set; }

        public int ChunkSize { get; set; }

        public long Offset { get; set; }
    }
}
