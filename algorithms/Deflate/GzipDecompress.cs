using Microsoft.VisualBasic;
using src.algorithms.HuffmanCoding;
using src.datastructures.BinarySearchTree;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using static System.Net.WebRequestMethods;
using System.Xml.Linq;
using File = System.IO.File;
using System.Buffers.Binary;

namespace src.algorithms.Deflate;

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

                // 2 bytes initialisation - gzip magic number: 
                var magicNr = reader.ReadUInt16();
                if (!(magicNr == (UInt16)0x1F8B || magicNr == (UInt16)35615)) return "Invalid gzip magic number.";
                // 1 byte compression-method -  Deflate = 8 for deflate
                byte compressionMethod = reader.ReadByte();
                if (compressionMethod != (Byte)8) return $"Unsupported compression method. Expected 8 got: [{compressionMethod}].";
                // 1 byte special-info - reserved-bits must be 0
                BitVector32 fileFlags = new BitVector32(reader.ReadByte());    //reader.ReadInt32() any difference?;
                if (fileFlags[5] || fileFlags[6] || fileFlags[7]) return "Reserved flags are set. Must be 0";
                // 4 byte unixtimestamp - last modified - time is in endian byte array -> we reverse it before casting it
                int modificationTime = BitConverter.ToInt32(reader.ReadBytes(4).ToArray());
                if (modificationTime != 0) Console.WriteLine($"Last modified - {modificationTime}");
                else Console.WriteLine("last modified - N/A");
                // 1 byte additional-info - info about kompression
                BitVector32 extraFlags = new BitVector32(reader.ReadByte());
                if (extraFlags[2]) Console.WriteLine("Compression - maximal Compression and slowest algorithm.");
                if (extraFlags[4]) Console.WriteLine("Compression - fastest Compression algorithm.");
                // 1 byte os-file-system - info about what OS this file was compressed on
                byte operatingSystem = reader.ReadByte();
                string os = operatingSystem switch
                {
                    0 => "FAT filesystem (MS-DOS, OS/2, NT/Win32)",
                    1 => "Amiga",
                    2 => "VMS (or OpenVMS)",
                    3 => "Unix",
                    4 => "VM/CMS",
                    5 => "Atari TOS",
                    6 => "HPFS filesystem (OS/2, NT)",
                    7 => "Macintosh",
                    8 => "Z-System",
                    9 => "CP/M",
                    10 => "TOPS-20",
                    11 => "NTFS filesystem (NT)",
                    12 => "QDOS",
                    13 => "Acorn RISCOS",
                    255 => "Unknown",
                    _ => "Could not match OperatingSystem Identifier",
                };
                Console.WriteLine($"File-System-Info - value: {operatingSystem} => {os}");
                // next come optinal flags, denoted in fileFlags.
                //  0x01    FTEXT       file is probably ASCII text.
                //  0x04    FEXTRA      The file contains extra fields
                //  0x08    FNAME       The file contains an original file name string
                //  0x10    FCOMMENT    The file contains comment
                //  0x20                Reserved
                //  0x40                Reserved
                //  0x80                Reserved
                if (fileFlags[0]) Console.WriteLine("Flag0 FTEXT - Indicating this is Text is set.");
                if (fileFlags[2]) 
                {
                    byte[] u16endian = reader.ReadBytes(2);
                    var bytesToSkipp = BinaryPrimitives.ReadUInt16LittleEndian(u16endian);
                    Console.WriteLine($"Flag2 FEXTRA - Indicating Extra");
                    reader.ReadBytes(bytesToSkipp);
                }
                if (fileFlags[3]) Console.WriteLine($"Flag3 FNAME- Indicating File name: {readNullTerminatedString(reader)}");
                if (fileFlags[4]) Console.WriteLine($"Flag4 FCOMMENT - Indicating Comment: {readNullTerminatedString(reader)}");
                if (fileFlags[1]) 
                {
                    reader.ReadBytes(2); // 2 byte checksum (that we just disregard)
                    Console.WriteLine("Flag1 FHCRC - Indicating this has a header-checksum is set.");
                }




            };
        }
        return "finished sucessfully";
    }

    private static string readNullTerminatedString(in BinaryReader reader)
    {
        char ch = reader.ReadChar();
        string result = "";
        while (ch != '\0')
        {
            result += ch;
        }
        return result;
    }

    private static 

}



/*
* 
*  ---         ---        ---
* 
*/
