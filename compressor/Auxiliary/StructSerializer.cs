using System.Runtime.InteropServices;

namespace Compressor.Auxiliary
{
    public class StructSerializer
    {
        public static byte[] Serialize<T>(T structObject) where T : struct
        {
            var structSize = Marshal.SizeOf(typeof(T));
            var bytes = new byte[structSize];
            var ptr = Marshal.AllocHGlobal(structSize);

            Marshal.StructureToPtr(structObject, ptr, true);
            Marshal.Copy(ptr, bytes, 0, structSize);
            Marshal.FreeHGlobal(ptr);

            return bytes;
        }

        public static T Deserialize<T>(byte[] bytes) where T : struct
        {
            var structSize = Marshal.SizeOf(typeof(T));
            var ptr = Marshal.AllocHGlobal(structSize);
            Marshal.Copy(bytes, 0, ptr, structSize);
            
            var structObject = (T)Marshal.PtrToStructure(ptr, typeof(T));
            Marshal.FreeHGlobal(ptr);

            return structObject;
        }

        public static int GetSize<T>() where T : struct
        {
            return Marshal.SizeOf(typeof(T));
        }
    }
}
