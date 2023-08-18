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
                Console.WriteLine("type: " + type);
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

        private void decompressHuffmanBlock(CanonicalHuffmanCode lenCode, CanonicalHuffmanCode? distCode)
        {
            while (true)
            {
                uint sym = lenCode.DecodeNextSymbol(this._input);
                if (sym < 256)
                {
                    // literal byte
                    this._output.WriteByte((byte)sym);
                    this._history.append((byte)sym);
                } else
                {
                    // length and distance used for copying
                    uint run = decodeRunLength(sym);
                    if (!(3 <= run && run <= 258)) throw new Exception("Invalid run length");
                    if (distCode is null) throw new Exception("Length symbol encountered with empty distance code.");
                    uint distSym = distCode.DecodeNextSymbol(this._input);
                    uint dist = this.decodeDistance(distSym);
                    if (!(1 <= dist && dist <= 32768)) throw new Exception("Invalid distance");
                    this._history.copy(dist, run, this._output);

                }
            }
        }

        private uint decodeRunLength(uint sym)
        {
            if (!(257 <= sym && sym <= 287))
                throw new ArgumentOutOfRangeException(nameof(sym), "Invalid run length symbol");
            if (sym <= 264) return sym - 254;
            else if (sym <= 284)
            {
                uint numExtraBits = (sym - 261) / 4;
                return (((sym - 265) % 4 + 4) << (int)numExtraBits) + 3 + _input.ReadUint(numExtraBits);
            }
            else if (sym == 285) return 258;
            else throw new InvalidDataException("Reserved length symbol: " + sym);
        }

        /// <summary>
        /// reads bits from the input stream to build the huffman-code that will be used for the following block
        /// </summary>
        /// <returns></returns>
        private  (CanonicalHuffmanCode lenCode, CanonicalHuffmanCode? distCode) readHuffmanCodes()
        {
            uint numLenCodes = this._input.ReadUint(5) + 257;   // hlit + 257
            uint numDisCodes = this._input.ReadUint(5) + 1;     // hdist + 1

            // read length the huffman-code takes in the stream
            uint numCodeLenCodes = this._input.ReadUint(4) + 4; // hclen + 4
            List<uint> codeLenCodeLen = new();
            // fill in fixed values:
            for (int i = 0; i < 19; i++)
                codeLenCodeLen.Add(0);
            codeLenCodeLen[16] = this._input.ReadUint(3);
            codeLenCodeLen[17] = this._input.ReadUint(3);
            codeLenCodeLen[18] = this._input.ReadUint(3);
            codeLenCodeLen[0] = this._input.ReadUint(3);
            for (int i=0; i<numCodeLenCodes-4; i++)
            {
                int j = (i % 2 == 0) ? (8 + i / 2) : (7 - i / 2);
                codeLenCodeLen[j] = this._input.ReadUint(3);
            }
            var codeLenCode = new CanonicalHuffmanCode(codeLenCodeLen.ToArray());

            // Read the main code lengths
            uint[] codeLens = new uint[numLenCodes + numDisCodes];
            for (uint codeLensIdx=0; codeLensIdx < codeLens.Length;)
            {
                uint sym = codeLenCode.DecodeNextSymbol(_input);
                if (0 <= sym && sym <= 15)
                {
                    codeLens[codeLensIdx] = sym;
                    codeLensIdx++;
                } else
                {
                    uint runLen;
                    uint runValue = 0;
                    if (sym == 16)
                    {
                        if (codeLensIdx == 0)
                            throw new FormatException("No code length value to copy.");
                        runLen = _input.ReadUint(2) + 3;
                        runValue = codeLens[codeLensIdx - 1];
                    }
                    else if (sym == 17)
                    {
                        runLen = _input.ReadUint(3) + 3;
                    }
                    else if (sym == 18)
                    {
                        runLen = _input.ReadUint(7) + 11;
                    }
                    else throw new InvalidDataException("Symbol out allowed of range.");
                    uint end = codeLensIdx + runLen;
                    if (end > codeLens.Length)
                        throw new InvalidDataException("Run exceeds numer of codes.");
                    Array.Fill(codeLens, codeLensIdx, (int)end, (int)runValue);
                    codeLensIdx = end;
                }
            }

            // create literal-length-code-tree
            uint[] lenCodeLen = codeLens[0 .. (int)numLenCodes];
            if (lenCodeLen[256] == 0)
                throw new InvalidDataException("End of block symbol has zero code length.");
            CanonicalHuffmanCode huffLenCode = new CanonicalHuffmanCode(lenCodeLen);

            //create distance-code-tree
            uint[] distCodesLen = codeLens[(int)numLenCodes .. codeLens.Length];
            CanonicalHuffmanCode? huffDistCode;
            if (distCodesLen.Length == 1 && distCodesLen[0] == 0)
                huffDistCode = null;    // no distance code -> the block will be all literal symbals
            else
            {
                // build statistics
                uint oneCount = 0;
                uint otherPositiveCOunt = 0;
                foreach (var x in distCodesLen)
                {
                    if (x == 1) oneCount++;
                    else if (x > 1) otherPositiveCOunt++;
                }
                // handle case: only one distance code is defined
                if (oneCount ==1 && otherPositiveCOunt == 0)
                {
                    // we need to fill dummy data in to make a complete huffman-tree (tree MUST always be valid)
                    // TODO...
                }
                huffDistCode = new CanonicalHuffmanCode(distCodesLen);
            }

            return (huffLenCode, huffDistCode);
        }

        private uint decodeDistance(uint sym)
        {
            if (!(0 <= sym && sym <= 31)) 
                throw new ArgumentOutOfRangeException(nameof(sym), "Invalid run length symbol");
            if (sym <= 3) return sym + 1;
            else if (sym <= 29)
            {
                uint numExtraBits = sym / 2 - 1;
                return ((sym % 2 +2) << (int)numExtraBits) + 1 + _input.ReadUint(numExtraBits);
            }
            else throw new InvalidDataException("Reserved length symbol: " + sym);
        }
        
        
        // building the huffman codes depending on BTYPE
        // BTYPE==1 -> static values
        private static CanonicalHuffmanCode makeFixedLenCode()
        {
            List<uint> codeLengths = new List<uint>(288);
            for (uint i =0; i<144; i++) codeLengths.Add(8);
            for (uint i =0; i<112; i++) codeLengths.Add(9);
            for (uint i =0; i<24; i++) codeLengths.Add(7);
            for (uint i =0; i<8; i++) codeLengths.Add(8);
            return new CanonicalHuffmanCode(codeLengths.ToArray());
        }

        private static CanonicalHuffmanCode makeFixedDistCode()
        {
            List<uint> codeLengths = new List<uint>(32);
            for (uint i = 0; i < 32; i++) codeLengths.Add(5);
            return new CanonicalHuffmanCode(codeLengths.ToArray());
        }
    }
}
