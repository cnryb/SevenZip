using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SevenZip;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            string input = "你好啊 ";
            var bytes = Encoding.UTF8.GetBytes(input);

            MemoryStream inStream = new MemoryStream(bytes);
            MemoryStream outStream = new MemoryStream();
            Zip(inStream, outStream);
            outStream.Position = 0;
            bytes = new byte[outStream.Length];
            outStream.Read(bytes, 0, bytes.Length);

            string str = Convert.ToBase64String(bytes);
            Console.WriteLine($"压缩后Base64编码 {str}");

            bytes = Convert.FromBase64String(str);
            inStream = new MemoryStream(bytes);
            outStream = new MemoryStream();
            UnZip(inStream, outStream);
            outStream.Position = 0;
            var sr = new StreamReader(outStream);
            str = sr.ReadToEnd();
            Console.WriteLine($"解压后内容 {str}");

        }

        static void Zip(Stream inStream, Stream outStream)
        {
            SevenZip.Compression.LZMA.Encoder encoder = new SevenZip.Compression.LZMA.Encoder();
            encoder.WriteCoderProperties(outStream);
            Int64 fileSize = inStream.Length;
            for (int i = 0; i < 8; i++)
                outStream.WriteByte((Byte)(fileSize >> (8 * i)));

            encoder.Code(inStream, outStream, -1, -1, null);
        }
        static void Zip(string inputName, string outputName)
        {
            var inStream = new FileStream(inputName, FileMode.Open, FileAccess.Read);
            var outStream = new FileStream(outputName, FileMode.Create, FileAccess.Write);
            Zip(inStream, outStream);
        }

        static void UnZip(string inputName, string outputName)
        {
            var inStream = new FileStream(inputName, FileMode.Open, FileAccess.Read);
            var outStream = new FileStream(outputName, FileMode.Create, FileAccess.Write);
            UnZip(inStream, outStream);
        }

        static void UnZip(Stream inStream, Stream outStream)
        {
            byte[] properties = new byte[5];
            if (inStream.Read(properties, 0, 5) != 5)
                throw (new Exception("input .lzma is too short"));
            SevenZip.Compression.LZMA.Decoder decoder = new SevenZip.Compression.LZMA.Decoder();
            decoder.SetDecoderProperties(properties);

            long outSize = 0;
            for (int i = 0; i < 8; i++)
            {
                int v = inStream.ReadByte();
                if (v < 0)
                    throw (new Exception("Can't Read 1"));
                outSize |= ((long)(byte)v) << (8 * i);
            }

            long compressedSize = inStream.Length - inStream.Position;
            decoder.Code(inStream, outStream, compressedSize, outSize, null);
        }
    }
}
