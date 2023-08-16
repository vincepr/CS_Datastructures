using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace src.algorithms.Deflate
{
    internal class ByteHistory
    {
        /// number of bytes written
        private int _length;
        /// circular ring buffer of bytes. (next input will overwrite oldest input when at capacity)
        private byte[] _data;
        /// index of the next byte to write to
        private int _index;

        /// <summary>
        /// Constructs a ring buffered array of bytes of specified size in bytes.
        /// </summary>
        /// <param name="size"></param>
        /// <exception cref="ArgumentOutOfRangeException"> is size is not positive</exception>
        public ByteHistory(int size)
        {
            if (size < 1) throw new ArgumentOutOfRangeException("size must be positive");
            _length = size;
            _data = new byte[size];
            _index = 0;
        }

        /// <summary>
        /// appends the given byte to this history.
        /// </summary>
        /// <param name="b"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void append(byte b)
        {
            if (0 > _index || _index >= _data.Length) throw new InvalidOperationException("Unreachable state in ByteHistory.append()");
            _data[_index] = b;
            _index = (_index + 1) % _data.Length;
            if (_length < _data.Length) _length++;
        }

        /// <summary>
        /// copies len bytes starting at dist bytes go to the output stream.
        /// </summary>
        /// <param name="dist"></param>
        /// <param name="len"></param>
        /// <param name="output"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public void copy(int dist, int len, Stream output)
        {
            if (len < 0 || dist < 1 || dist > _length) throw new ArgumentOutOfRangeException("Invalid length or distance");
            int readIdx = (_index - dist + _data.Length) % _data.Length;
            if (0 > readIdx || readIdx >= _data.Length) throw new InvalidOperationException("Unreachable state in ByteHistory.copy()");
            for (int i=0; i<len; i++)
            {
                var by = _data[readIdx];
                ReadOnlySpan<byte> b = new byte[] { _data[readIdx] };
                readIdx = (readIdx + 1) % _data.Length;
                output.Write(b);
                append(by);
            }

        }
    }
}
