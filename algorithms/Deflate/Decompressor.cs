using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace src.algorithms.Deflate
{
    internal class Decompressor
    {
        const int SIZE_OF_HISTORY_IN_BYTES = 32 * 1024;

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
            _history = new ByteHistory(SIZE_OF_HISTORY_IN_BYTES);

            // Process of decompression:
            bool isFinal;
            {
                // Header Block
                isFinal = input.readUint(1) != 0;       // BFINAL
                uint type = input.readUint(2);          // BTYPE

                // decompress rest of the block depending on type
                if (type == 0)
                {
                    decompressUncompressedBlock();
                }
                else if (type == 1)
                {
                    decompressHuffmanBlock(FIXED_LENGTH, FIXED_DISTANCE);
                }
                else if (type == 2)
                {
                    CanonicalCode[] hufLenDist = decodeHuffmanCodes();
                    decompressHuffmanBlock();
                }
                else if (type == 3)
                {
                    throw new InvalidDataException("Reserved block type.");
                }
                else throw new InvalidDataException("Unreachable");
            } while (!isFinal) ;

        }

        private void decompressUncompressedBlock()
        {
            throw new NotImplementedException();
        }

        private decompressHuffmanBlock()
        {
            throw new NotImplementedException();
        }
    }
}
