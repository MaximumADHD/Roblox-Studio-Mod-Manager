// Forked from:
// https://github.com/pizzaboxer/rbxfflagdumper/blob/main/RbxFFlagDumper.Lib/PatternScanner.cs

using System.Collections.Generic;
using System.Globalization;
using System;

namespace RobloxStudioModManager
{
    public class PatternScanner
    {
        private readonly byte[] _binary;
        private readonly List<byte?> _pattern = new List<byte?>();

        private static readonly IFormatProvider _invariant = NumberFormatInfo.InvariantInfo;
        private static readonly NumberStyles _hexNumber = NumberStyles.HexNumber;

        private readonly int _end;
        private int _pos = 0;

        public PatternScanner(byte[] binary, string patternStr, int start, int length)
        {
            _binary = binary;
            _pos = start;

            foreach (string bit in patternStr.Split(' '))
            {
                byte? value = null;

                if (byte.TryParse(bit, _hexNumber, _invariant, out byte result))
                     value = result;

                _pattern.Add(value);

            }    
            
            _end = start + length - _pattern.Count;
        }

        public PatternScanner(byte[] binary, string patternStr)
            : this(binary, patternStr, 0, binary.Length)
        {
        }

        public int FindNext()
        {
            while (!Finished)
            {
                for (int i = 0; i < _pattern.Count; i++)
                {
                    if (!(_pattern[i] is null))
                        if (_binary[_pos+i] != _pattern[i])
                            break;

                    if (i == _pattern.Count-1)
                    {
                        int result = _pos;
                        _pos += _pattern.Count;
                        return result;
                    }
                }

                _pos++;
            }

            return Finished ? _pos : -1;
        }

        public bool Finished => _pos >= _end;
        public void Reset() => _pos = 0;
    }
}
