using src.algorithms.HuffmanCoding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace src.algorithms.Deflate
{
    internal class GzipDecompress
    {
        public static string GzipRun(string[] args)
        {
            if (args.Length != 2) return "Usage: GzipDecompress Inputfile.gz Outputfile";
            string inPath = args[0];

            if (!File.Exists(inPath) || Directory.Exists(inPath)) return $"Input-File {inPath} not found!";

            const int bufferSize = 16 * 1024;

            var options = new FileStreamOptions
            {
                Mode = FileMode.Open,
                Access = FileAccess.Read,
                BufferSize = bufferSize,
                // PreallocationSize = bufferSize,
            };

            using (var stream = File.OpenRead(inPath))
            {
                // StreamReader vs BinaryReader here?
                using (var reader = new BinaryReader(stream))
                {
                    /* we can only use this to adjust encoding if using StreamReader
                    reader.Peek();
                    var encoding = reader.CurrentEncoding;
                    */

                    // 2 bytes = 16 bits gzip magic number: 
                    if (reader.ReadInt16() != 0x1F8B) return "Invalid gzip magic number.";
                    int compressionMethod =reader.ReadInt32();
                    if (compressionMethod != 8) return $"Unsupported compression method. Expected 8 got: [{compressionMethod}].";
                    BitVector32 flags = new BitVector32(reader.ReadByte());    //reader.ReadInt32() any difference?;
                    var x =flags[1]


                };
            }
            



                return "finished sucessfully";
        }

    }

}






/*
 * 
 *  ---         ---        ---
 * 
 */
