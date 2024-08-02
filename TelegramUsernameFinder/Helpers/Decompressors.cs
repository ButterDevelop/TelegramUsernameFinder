using System.IO.Compression;
using System.Text;

namespace TelegramUsernameFinder.Helpers
{
    public class Decompressors
    {
        public static string DecompressGzip(byte[] data)
        {
            using (var compressedStream   = new MemoryStream(data))
            using (var decompressedStream = new MemoryStream())
            using (var gzipStream         = new GZipStream(compressedStream, CompressionMode.Decompress))
            {
                gzipStream.CopyTo(decompressedStream);
                return Encoding.UTF8.GetString(decompressedStream.ToArray());
            }
        }

        public static string DecompressDeflate(byte[] data)
        {
            using (var compressedStream   = new MemoryStream(data))
            using (var decompressedStream = new MemoryStream())
            using (var deflateStream      = new DeflateStream(compressedStream, CompressionMode.Decompress))
            {
                deflateStream.CopyTo(decompressedStream);
                return Encoding.UTF8.GetString(decompressedStream.ToArray());
            }
        }

        public static string DecompressBrotli(byte[] data)
        {
            using (var compressedStream   = new MemoryStream(data))
            using (var decompressedStream = new MemoryStream())
            using (var brotliStream       = new BrotliStream(compressedStream, CompressionMode.Decompress))
            {
                brotliStream.CopyTo(decompressedStream);
                return Encoding.UTF8.GetString(decompressedStream.ToArray());
            }
        }
    }
}
