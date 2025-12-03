// Forked from:
// https://github.com/pizzaboxer/rbxfflagdumper/blob/main/RbxFFlagDumper.Lib/StudioFFlagDumper.cs

using PeNet;
using PeNet.Header.Pe;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RobloxStudioModManager
{
    public static class StudioFFlagDumper
    {
        private static int GetRVAOffset(ImageSectionHeader sectionHeader)
            => (int)(sectionHeader.VirtualAddress - sectionHeader.PointerToRawData);

        public static List<string> DumpAllFlags(string studioPath)
        {
            var list = new List<string>();

            var cppFlags = DumpCppFlags(Path.Combine(studioPath, "RobloxStudioBeta.exe"));
            var luaFlags = DumpLuaFlags(Path.Combine(studioPath, "ExtraContent"));
            var commonFlags = cppFlags.Intersect(luaFlags);

            list.AddRange(cppFlags.Where(x => !commonFlags.Contains(x)).Select(x => "[C++] " + x));
            list.AddRange(luaFlags.Where(x => !commonFlags.Contains(x)).Select(x => "[Lua] " + x));
            list.AddRange(commonFlags.Select(x => "[Com] " + x));

            return list.OrderBy(x => x.Substring(6)).ToList();
        }

        // TODO: Look for relevant calls in parsed luac files.
        public static List<string> DumpLuaFlags(string extraContentPath)
        {
            var finalList = new List<string>();

            foreach (var file in Directory.GetFiles(extraContentPath, "*.lua", SearchOption.AllDirectories))
            {
                string contents = File.ReadAllText(file);

                var matches = Regex.Matches(contents, "game:(?:Get|Define)Fast(Flag|Int|String)\\(\\\"(\\w+)\\\"\\)").Cast<Match>();
                var userMatches = Regex.Matches(contents, "(?:IsUserFeatureEnabled|getUserFlag)\\(\\\"(\\w+)\\\"\\)").Cast<Match>();

                foreach (var match in matches)
                {
                    string flag = string.Format("F{0}{1}", match.Groups[1], match.Groups[2]);

                    if (!finalList.Contains(flag))
                        finalList.Add(flag);
                }

                foreach (var match in userMatches)
                {
                    string flag = string.Format("FFlag{0}", match.Groups[1]);

                    if (!finalList.Contains(flag) && flag != "FFlagUserDoStuff")
                        finalList.Add(flag);
                }
            }

            finalList.Sort();
            return finalList;
        }

        /// <summary>
        /// Dumps all C++ defined flags found within the RobloxStudioBeta executable
        /// </summary>
        /// <param name="studioExePath"></param>
        /// <returns></returns>
        /// <exception cref="CppDumpException">
        /// Thrown when an issue with C++ dumping occurs.
        /// </exception>
        public static List<string> DumpCppFlags(string studioExePath)
        {
            byte[] binary = File.ReadAllBytes(studioExePath);
            var exe = new PeFile(binary);

            var sectionHeaders = exe.ImageSectionHeaders;
            var textHeader = sectionHeaders.First(x => x.Name == ".text");
            var rdataHeader = sectionHeaders.First(x => x.Name == ".rdata");
            int rvaOffset = GetRVAOffset(textHeader) - GetRVAOffset(rdataHeader);

            var textStart = (int)textHeader.PointerToRawData;
            var textSize = (int)textHeader.SizeOfRawData;

            // this snippet is present for each registered fflag in RobloxStudioBeta.exe
            // 00:  41 B8 ?? ?? ?? ??    | mov r8d, <val>     ; byte, determines if dynamic
            // 06:  48 8D 15 ?? ?? ?? ?? | lea rdx, <addr>    ; 
            // 13:  48 8D 0D ?? ?? ?? ?? | lea rcx, <addr>    ; fflag name
            // 20:  E9 ?? ?? ?? ??       | jmp <addr>         ; registration routine, determines data type (FFlag, SFFlag, FString, etc)

            const string basePattern = "41 B8 ?? 00 00 00 48 8D 15 ?? ?? ?? ?? 48 8D 0D ?? ?? ?? ?? E9";
            var baseScanner = new PatternScanner(binary, basePattern, textStart, textSize);

            // this snippet is present at the bottom of version'd fflag declarations
            // everything that precedes up to the fflag address is variable,
            // but is a mixture of the instructions: mov, movsd, movzx, and movups
            //
            // 00:  BA ?? ?? ?? ??
            // 05:  48 8D 4C 24 20
            // 10:  E8 ?? ?? ?? ??

            const string endPattern = "BA ?? ?? ?? ?? 48 8D 4C 24 20 E8 ?? ?? ?? ??";
            var endScanner = new PatternScanner(binary, endPattern, textStart, textSize);

            var dataTypeTable = new Dictionary<int, string>();
            var rawFFlagData = new List<RawFFlagData>();

            while (!baseScanner.Finished)
            {
                int pos = baseScanner.FindNext();

                if (pos == -1)
                    break;

                int param = binary[pos + 2];

                // resolving the pointer with a constant offset since we can just assume it will always point to .rdata
                var nameOffset = pos + 20 + BitConverter.ToInt32(binary, pos + 16) + rvaOffset;
                var targetOffset = pos + 25 + BitConverter.ToInt32(binary, pos + 21);
                var namebuf = new List<byte>();
                var bad = false;

                for (var i = nameOffset; binary[i] != 0; i++)
                {
                    var value = binary[i];

                    if (value > 127 || value < 30)
                    {
                        bad = true;
                        break;
                    }

                    namebuf.Add(value);
                }

                if (!bad)
                {
                    var name = Encoding.UTF8.GetString(namebuf.ToArray());
                    rawFFlagData.Add(new RawFFlagData(targetOffset, param, name));

                    if (dataTypeTable.ContainsKey(targetOffset))
                        continue;

                    dataTypeTable[targetOffset] = null;
                }
            }

            // currently only two SFFlags exist - would there ever be zero?
            if (dataTypeTable.Count != 5)
                throw new Exception("Expected 5 different flag types");

            // the registration routines for each flag type are stored in memory consecutively in this order
            dataTypeTable = dataTypeTable.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
            dataTypeTable[dataTypeTable.Keys.ElementAt(0)] = "FFlag";
            dataTypeTable[dataTypeTable.Keys.ElementAt(1)] = "SFFlag";
            dataTypeTable[dataTypeTable.Keys.ElementAt(2)] = "FInt";
            dataTypeTable[dataTypeTable.Keys.ElementAt(3)] = "FLog";
            dataTypeTable[dataTypeTable.Keys.ElementAt(4)] = "FString";

            // collect fflag versions
            var versions = new Dictionary<string, int>();

            while (!endScanner.Finished)
            {
                int endOffset = endScanner.FindNext();

                if (endOffset < 0)
                    break;

                // Extract version number at offset + 1
                int version = BitConverter.ToInt32(binary, endOffset + 1);

                // Work backwards to find first RIP-relative load
                // Look up to 200 bytes back to handle long strings

                int searchStart = Math.Max(0, endOffset - 200);
                int firstLoadOffset = -1;
                int firstLoadLength = 0;

                for (int i = endOffset - 1; i >= searchStart; i--)
                {
                    if (i + 7 > binary.Length)
                        continue;

                    byte b0 = binary[i];
                    byte b1 = binary[i + 1];
                    byte b2 = binary[i + 2];

                    bool isRipRelative = false;
                    int instrLength = 0;

                    // movups xmm, [rip+offset]: 0F 10 05
                    if (b0 == 0x0F && b1 == 0x10 && b2 == 0x05)
                    {
                        isRipRelative = true;
                        instrLength = 7;
                    }
                    // movsd xmm, [rip+offset]: F2 0F 10 05
                    else if (b0 == 0xF2 && b1 == 0x0F && b2 == 0x10 && i + 8 <= binary.Length && binary[i + 3] == 0x05)
                    {
                        isRipRelative = true;
                        instrLength = 8;
                    }
                    // movzx reg, byte/word ptr [rip+offset]: 0F B6/B7 05
                    else if (b0 == 0x0F && (b1 == 0xB6 || b1 == 0xB7) && b2 == 0x05)
                    {
                        isRipRelative = true;
                        instrLength = 7;
                    }
                    // mov reg, [rip+offset]: 8B 0D
                    else if (b0 == 0x8B && b1 == 0x0D)
                    {
                        isRipRelative = true;
                        instrLength = 6;
                    }

                    if (isRipRelative)
                    {
                        firstLoadOffset = i;
                        firstLoadLength = instrLength;
                    }
                }

                if (firstLoadOffset < 0)
                    continue;

                // Parse the RIP-relative address
                using (var stream = new MemoryStream(binary))
                using (var reader = new BinaryReader(stream))
                {
                    // The offset is always the last 4 bytes of the instruction
                    int offsetPosition = firstLoadOffset + firstLoadLength - 4;
                    stream.Position = offsetPosition;

                    // Calculate string RVA: next instruction + offset
                    int relativeOffset = reader.ReadInt32();
                    var nameOffset = stream.Position + rvaOffset + relativeOffset;

                    if (nameOffset < 0 || nameOffset >= binary.Length)
                        continue;

                    var nameBuffer = new List<byte>();
                    var bad = false;

                    for (var i = nameOffset; binary[i] != 0; i++)
                    {
                        var value = binary[i];

                        if (value > 127 || value < 30)
                        {
                            bad = true;
                            break;
                        }

                        nameBuffer.Add(value);
                    }

                    if (!bad)
                    {
                        string name = Encoding.UTF8.GetString(nameBuffer.ToArray());

                        if (string.IsNullOrWhiteSpace(name))
                            continue;

                        if (versions.ContainsKey(name))
                            continue;

                        versions.Add(name, version);
                    }
                }
            }

            var finalList = new List<string>();

            foreach (var entry in rawFFlagData)
            {
                string dataType = dataTypeTable[entry.DataTypeId];
                string name = "";

                if (entry.ByteParam == 2)
                    name += "D";

                name += dataType;
                name += entry.Name;

                if (versions.TryGetValue(entry.Name, out int version))
                    name += version;

                finalList.Add(name);
            }

            finalList.Sort();
            return finalList;
        }
    }
}
