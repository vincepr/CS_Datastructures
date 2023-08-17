using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace src.algorithms.Deflate
{
    // constants for static huffman codes (btype==1)
    
    
    internal class Decompressor
    {
        private static CanonicalHuffmanCode FIXED_LENGTH_CODE = Decompressor.makeFixedLenCode();
        private static CanonicalHuffmanCode FIXED_DIST_CODE = Decompressor.makeFixedDistCode();
        
        private const int SizeOfHistoryInBytes = 32 * 1024;

        private BitStream _input;
        private Stream _output;
        private ByteHistory _history;

        /// <summary>
        /// starts the decompression process for data in the inputstream and writes it to the output stream.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        public static void Decompress(BitStream input, Stream output)
        {
            new Decompressor(input, output);
        }

        /// <summary>
        /// Constructor, that immediatly starts the decompressoin.
        /// </summary>
        private Decompressor(BitStream input, Stream output)
        {
            _input = input;
            _output = output;
            _history = new ByteHistory(SizeOfHistoryInBytes);

            // Process of decompression:
            bool isFinal;
            {
                // Header Block
                isFinal = input.ReadUint(1) != 0;       // BFINAL
                uint type = input.ReadUint(2);          // BTYPE

                // decompress rest of the block depending on type
                if (type == 0)
                {
                    decompressUncompressedBlock();
                }
                else if (type == 1)
                {
                    decompressHuffmanBlock(FIXED_LENGTH_CODE, FIXED_DIST_CODE);
                }
                else if (type == 2)
                {
                    var(huffLenCode, huffDistCode) = readHuffmanCodes();
                    decompressHuffmanBlock(huffLenCode, huffDistCode);
                }
                else if (type == 3)
                {
                    throw new InvalidDataException("Reserved block type.");
                }
                else throw new InvalidDataException("Unreachable");
            } while (!isFinal);

        }

        private void decompressUncompressedBlock()
        {
            throw new NotImplementedException();
        }

        private void decompressHuffmanBlock(CanonicalHuffmanCode lenCode, CanonicalHuffmanCode distCode)
        {
            throw new NotImplementedException();
        }

        private  (CanonicalHuffmanCode lenCode, CanonicalHuffmanCode distCode) readHuffmanCodes()
        {
            throw new NotImplementedException();
        }
        
        
        // building the huffman codes depending on BTYPE
        // BTYPE==1 -> static values
        private static CanonicalHuffmanCode makeFixedLenCode()
        {
            List<int> codeLengths = new List<int>(288);
            for (int i=0; i<144; i++) codeLengths.Add(8);
            for (int i=0; i<112; i++) codeLengths.Add(9);
            for (int i=0; i<24; i++) codeLengths.Add(7);
            for (int i=0; i<8; i++) codeLengths.Add(8);
            return new CanonicalHuffmanCode(codeLengths.ToArray());
        }

        private static CanonicalHuffmanCode makeFixedDistCode()
        {
            List<int> codeLengths = new List<int>(32);
            for (int i = 0; i < 32; i++) codeLengths.Add(5);
            return new CanonicalHuffmanCode(codeLengths.ToArray());
        }
    }
}
