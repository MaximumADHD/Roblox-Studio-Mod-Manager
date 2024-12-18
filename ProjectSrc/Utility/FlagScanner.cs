using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RobloxStudioModManager
{
    public class FlagScanner
    {
        /// https://stackoverflow.com/a/39021296/11852173
        /// <summary>Looks for the next occurrence of a sequence in a byte array</summary>
        /// <param name="array">Array that will be scanned</param>
        /// <param name="start">Index in the array at which scanning will begin</param>
        /// <param name="sequence">Sequence the array will be scanned for</param>
        /// <returns>
        ///   The index of the next occurrence of the sequence of -1 if not found
        /// </returns>
        private static int findSequence(byte[] array, int start, byte[] sequence)
        {
            int end = array.Length - sequence.Length; // past here no match is possible
            byte firstByte = sequence[0]; // cached to tell compiler there's no aliasing

            while (start <= end)
            {
                // scan for first byte only. compiler-friendly.
                if (array[start] == firstByte)
                {
                    // scan for rest of sequence
                    for (int offset = 1; ; ++offset)
                    {
                        if (offset == sequence.Length)
                        {
                            // full sequence matched?
                            return start;
                        }
                        else if (array[start + offset] != sequence[offset])
                        {
                            break;
                        }
                    }
                }

                ++start;
            }

            // end of array reached without match
            return -1;
        }

        private static readonly List<byte[]> opcodes = new List<byte[]>
        {
            new byte[] { 0xE9 },
            new byte[] { 0x48, 0x8D, 0x0D }
        };

        private static int ResolveInstTargetAddr(byte[] binary, int pos, int dataOffset = 0)
        {
            int len = 0;

            foreach (var opcode in opcodes)
            {
                if (findSequence(binary, pos, opcode) == pos)
                {
                    len = opcode.Length;
                    break;
                }
            }

            if (len == 0)
                return -1;

            return pos + len + 4 + BitConverter.ToInt32(binary, pos + len) + dataOffset;
        }

        public static void PerformScan(string studioPath, HashSet<string> flags, Action<string> print = null)
        {
            var binary = File.ReadAllBytes(studioPath);

            var knownAddresses = new List<int>
            {
                findSequence(binary, 0, Encoding.UTF8.GetBytes("DebugDisplayFPS")),
                findSequence(binary, 0, Encoding.UTF8.GetBytes("DebugGraphicsPreferVulkan")),
                findSequence(binary, 0, Encoding.UTF8.GetBytes("DebugGraphicsPreferD3D11"))
            };

            knownAddresses = knownAddresses.Where(x => x != -1).ToList();

            if (knownAddresses.Count < 2)
                throw new Exception("Could not find address(es) of known flag");

            // https://files.pizzaboxer.xyz/i/x64dbg_PavTJp2sLp.png
            // Note that for instructions that handle addresses (e.g. lea and jmp), operand is
            // an offset to a memory address relative to the address of the instruction's end
            // mov r8d, <value>       | 41 B8 ?? ?? ?? ??    | Determines if it's a dynamic flag
            // lea rdx, ds:[<offset>] | 48 8D 15 ?? ?? ?? ?? | Loads the default flag value
            // lea rcx, ds:[<offset>] | 48 8D 0D ?? ?? ?? ?? | Loads the string of the flag name into memory 
            // jmp <offset>           | E9 ?? ?? ?? ??       | Jumps to the subroutine that registers it as a flag

            int position = 0;
            int knownLeaInstAddr = 0;
            var possibleOffsets = new Dictionary<int, int>();

            while (position < binary.Length)
            {
                // Look for the 'lea rcx' instruction
                int leaInstAddr = findSequence(binary, position, new byte[] { 0x48, 0x8D, 0x0D });

                if (leaInstAddr == -1)
                    break;

                // Next instruction should be a 'jmp'
                // yes this is essentially just a really bad pattern scan
                if (binary[leaInstAddr + 7] != 0xE9)
                {
                    position = leaInstAddr + 3;
                    continue;
                }

                int leaTargetAddr = ResolveInstTargetAddr(binary, leaInstAddr);

                // Weird oddity - the target address specified by the lea instruction may
                // itself be offset up to 0x0F00 bytes prior, with an alignment of 0x0100
                for (int i = 0; i > -0xFF00; i -= 0x0100)
                {
                    foreach (int knownAddress in knownAddresses)
                    {
                        if (leaTargetAddr + i != knownAddress)
                            continue;

                        if (possibleOffsets.ContainsKey(i))
                            possibleOffsets[i]++;
                        else
                            possibleOffsets.Add(i, 1);

                        if (knownLeaInstAddr != 0)
                            continue;

                        knownLeaInstAddr = leaInstAddr;
                    }
                }

                position = leaInstAddr + 3;
            }

            print?.Invoke("Finished scanning binary");
            var validOffsets = possibleOffsets.Where(x => x.Value == knownAddresses.Count);

            if (validOffsets.Count() != 1)
                throw new Exception("Could not find correct data address offset");

            int dataAddrOffset = validOffsets.First().Key;

            // After the lea instruction comes a jmp instruction
            // The target address of this instruction is a subroutine which all registered
            // flags go through
            // There are separate subroutines for each type (FFlag, FInt, FString, etc)
            // which are all aligned by 0x20

            int jmpTargetAddr = ResolveInstTargetAddr(binary, knownLeaInstAddr + 7);

            var typeAddresses = new Dictionary<int, string>
            {
                { jmpTargetAddr,                   "FFlag"   },
                { jmpTargetAddr + 0x40,            "SFFlag"  },
                { jmpTargetAddr + 0x40 + 0x20,     "FInt"    },
                { jmpTargetAddr + 0x40 + 0x20 * 2, "FLog"    },
                { jmpTargetAddr + 0x40 + 0x20 * 3, "FString" },
            };

            position = 0;

            while (position < binary.Length)
            {
                int jmpInstAddr = findSequence(binary, position, new byte[] { 0xE9 });

                if (jmpInstAddr == -1)
                    break;

                jmpTargetAddr = ResolveInstTargetAddr(binary, jmpInstAddr);

                if (!typeAddresses.TryGetValue(jmpTargetAddr, out string flagType))
                {
                    position = jmpInstAddr + 1;
                    continue;
                }

                int targetLeaAddress = ResolveInstTargetAddr(binary, jmpInstAddr - 7, dataAddrOffset);
                string flagName = "";

                // Check if it's a dynamic flag
                // The operand of the 'mov' instruction will be 0x2 if it is
                if (flagType != "SFFlag" && binary[jmpInstAddr - 18] == 0x2)
                    flagName += 'D';

                flagName += flagType;

                for (int i = targetLeaAddress; binary[i] != 0; i++)
                {
                    // ascii control characters - if we encounter these, something has gone seriously wrong.

                    if (binary[i] < 0x20 || binary[i] > 0x7F)
                        throw new Exception("Encountered invalid data");

                    flagName += Convert.ToChar(binary[i]);
                }

                flags.Add(flagName);
                position = jmpInstAddr + 1;
            }
        }
    }
}
