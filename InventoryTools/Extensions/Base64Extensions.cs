using System;
using System.IO;
using System.IO.Compression;

namespace InventoryTools.Extensions
{
    public static class Base64Extensions
    {
        public static string ToCompressedBase64(this byte[] data)
        {
            using var compressedStream = new MemoryStream();
            using (var zipStream = new GZipStream(compressedStream, CompressionMode.Compress))
            {
                zipStream.Write(data, 0, data.Length);
            }

            return Convert.ToBase64String(compressedStream.ToArray());
        }
        
        public static byte[] FromCompressedBase64(this string compressedBase64)
        {
            var       data             = Convert.FromBase64String(compressedBase64);
            using var compressedStream = new MemoryStream(data);
            using var zipStream        = new GZipStream(compressedStream, CompressionMode.Decompress);
            using var resultStream     = new MemoryStream();
            zipStream.CopyTo(resultStream);
            return resultStream.ToArray();
        }
    }
}