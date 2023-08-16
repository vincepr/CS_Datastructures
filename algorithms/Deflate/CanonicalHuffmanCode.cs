using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace src.algorithms.Deflate
{
    internal class CanonicalHuffmanCode
    {
        private const int MAX_CODE_LENGTH = 15;
        private readonly Dictionary<int, int> _bitToSymbol;

        public CanonicalHuffmanCode(in int[] codeLengths)
        {
            // check if params are of valid state:
            foreach( var l in codeLengths)
            {
                if (l < 0) throw new ArgumentOutOfRangeException("Negative code length");
                if (l > MAX_CODE_LENGTH) throw new ArgumentOutOfRangeException("Maximum code length exceeded.");
            }

            //
            int nextCode = 0;
            for (int codeLen=1; codeLen <= MAX_CODE_LENGTH; codeLen++)
            {
                nextCode = nextCode << 1;
                int startBit = 1 << codeLen;
                for (int symbol = 0; symbol < codeLengths.Length; symbol++)
                {
                    if (codeLengths[symbol] != codeLen) continue;
                    if (nextCode >= startBit) throw new Exception("Canonical code produces illegal OVER-full Huffman-code-tree.");
                    _bitToSymbol[startBit | nextCode] = symbol;
                    nextCode++;
                }
            }
            if (nextCode != 1 << MAX_CODE_LENGTH) throw new Exception("Canonical code produces illegal UNDER-full Huffman-code-tree.");
        }


    }
}
