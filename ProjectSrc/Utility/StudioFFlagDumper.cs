// Forked from:
// https://github.com/pizzaboxer/rbxfflagdumper/blob/main/RbxFFlagDumper.Lib/StudioFFlagDumper.cs

using PeNet;
using PeNet.Header.Pe;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RobloxStudioModManager
{
    public class CppFlagEntry
    {
        public string Name;
        public string FullName;
        public uint Type;
        public ulong Variable;
    }

    public class CppFlagScan
    {
        public byte[] Binary;
        public ulong ImageBase;
        public ImageSectionHeader Text;
        public ImageSectionHeader RData;
        public ImageSectionHeader Data;
        public List<CppFlagEntry> Flags;
    }

    public static class StudioFFlagDumper
    {
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

                    if (!finalList.Contains(flag))
                    {
                        finalList.Add(flag);
                    }
                }
            }

            finalList.Sort();
            return finalList;
        }

        private static readonly Regex luaDefineRegex = new Regex
        (
            "game:DefineFast(Flag|Int|String)\\(\\s*\"(\\w+)\"\\s*,\\s*(true|false|-?\\d+|\"(?:[^\"\\\\]|\\\\.)*\")\\s*\\)",
            RegexOptions.Compiled
        );

        /// <summary>
        /// Dumps the default values of flags defined via game:DefineFast*(name, default) in ExtraContent Lua files.
        /// </summary>
        public static Dictionary<string, string> DumpLuaFlagDefaults(string extraContentPath)
        {
            var defaults = new Dictionary<string, string>();

            foreach (var file in Directory.GetFiles(extraContentPath, "*.lua", SearchOption.AllDirectories))
            {
                string contents = File.ReadAllText(file);

                foreach (Match match in luaDefineRegex.Matches(contents))
                {
                    string flag = $"F{match.Groups[1]}{match.Groups[2]}";
                    string value = match.Groups[3].Value;

                    if (value.StartsWith("\"", StringComparison.Ordinal))
                        value = value.Substring(1, value.Length - 2)
                            .Replace("\\\"", "\"")
                            .Replace("\\\\", "\\");

                    defaults.TryAdd(flag, value);
                }
            }

            return defaults;
        }


        private const uint TYPE_STRING = 3;
        private const uint TYPE_INT = 2;
        private const uint TYPE_LOG = 4;

        /// <summary>
        /// Dumps all C++ defined flags found within the RobloxStudioBeta executable
        /// </summary>
        /// <param name="studioExePath"></param>
        /// <returns></returns>
        public static List<string> DumpCppFlags(string studioExePath)
        {
            var scan = ScanCppFlags(studioExePath);

            var finalList = scan.Flags
                .Select(flag => flag.FullName)
                .ToList();

            finalList.Sort();
            return finalList;
        }

        /// <summary>
        /// Dumps the engine default value of every C++ defined flag in the RobloxStudioBeta executable.
        /// Flags whose default could not be determined statically are mapped to null.
        /// </summary>
        public static Dictionary<string, string> DumpCppFlagDefaults(string studioExePath)
        {
            var scan = ScanCppFlags(studioExePath);
            var defaults = new Dictionary<string, string>();
            var strings = new List<CppFlagEntry>();

            foreach (var flag in scan.Flags)
            {
                if (flag.Type == TYPE_STRING)
                {
                    strings.Add(flag);
                    continue;
                }

                // Variables in .bss (past the raw data) are zero-initialized.
                long offset = getDataOffset(scan, flag.Variable, 8);
                byte[] binary = scan.Binary;

                switch (flag.Type)
                {
                    case TYPE_INT:
                    {
                        int value = offset >= 0
                            ? BitConverter.ToInt32(binary, (int)offset)
                            : 0;

                        defaults[flag.FullName] = value.ToString(CultureInfo.InvariantCulture);
                        break;
                    }
                    case TYPE_LOG:
                    {
                        byte value = binary[offset];

                        defaults[flag.FullName] = value >= 0
                            ? value.ToString(CultureInfo.InvariantCulture)
                            : "0";
                        
                        break;
                    }
                    default:
                    {
                        bool value = offset >= 0 && binary[offset] != 0;
                        defaults[flag.FullName] = value ? "true" : "false";
                        break;
                    }
                }
            }

            foreach (var pair in dumpStringDefaults(scan, strings))
                defaults[pair.Key] = pair.Value;

            return defaults;
        }

        private static long getDataOffset(CppFlagScan scan, ulong va, int length)
        {
            var data = scan.Data;
            ulong start = scan.ImageBase + data.VirtualAddress;

            if (va < start)
                return -1;

            ulong rel = va - start;

            if (rel + (ulong)length > data.SizeOfRawData)
                return -1;

            long offset = data.PointerToRawData + (long)rel;
            return offset + length <= scan.Binary.Length ? offset : -1;
        }

        private static bool isPrintable(byte value) => value >= 32 && value < 127;

        // Returns the C string at the given RVA if it lies within .rdata and is exactly `length` printable characters.
        private static string readRDataString(CppFlagScan scan, long rva, int length)
        {
            var rdata = scan.RData;

            if (rva < rdata.VirtualAddress || rva + length >= rdata.VirtualAddress + rdata.SizeOfRawData)
                return null;

            long offset = rva - rdata.VirtualAddress + rdata.PointerToRawData;
            byte[] binary = scan.Binary;

            if (binary[offset + length] != 0)
                return null;

            for (long i = offset; i < offset + length; i++)
            {
                if (isPrintable(binary[i]))
                    continue;

                return null;
            }

            return Encoding.ASCII.GetString(binary, (int)offset, length);
        }

        // FString variables are MSVC std::string objects: { char buf[16] / char* ptr, size_t size, size_t capacity }.
        // Their contents are written by dynamic initializers that copy from a string literal in .rdata,
        // so we locate the code referencing each variable and find the literal of the expected length it reads.
        private static Dictionary<string, string> dumpStringDefaults(CppFlagScan scan, List<CppFlagEntry> strings)
        {
            const int VAR_SPAN = 40;
            const int SEARCH_WINDOW = 96;

            var result = new Dictionary<string, string>();

            if (strings.Count == 0)
                return result;

            byte[] binary = scan.Binary;
            ulong imageBase = scan.ImageBase;

            var sorted = strings
                .OrderBy(flag => flag.Variable)
                .ToArray();

            var varRvas = sorted
                .Select(flag => (long)(flag.Variable - imageBase))
                .ToArray();

            long minVar = varRvas[0];
            long maxVar = varRvas[varRvas.Length - 1] + VAR_SPAN;

            var text = scan.Text;
            int textOffset = (int)text.PointerToRawData;
            int textLength = (int)Math.Min(text.SizeOfRawData, (uint)(binary.Length - textOffset));

            long textRva = text.VirtualAddress;
            long ripTarget(int pos) => textRva + pos + 4 + BitConverter.ToInt32(binary, textOffset + pos);

            // (position of disp32 in .text, offset into the variable)
            var xrefs = new List<(int Pos, int Offset)>[sorted.Length];

            for (int pos = 0; pos < textLength - 4; pos++)
            {
                long target = ripTarget(pos);

                if (target < minVar || target >= maxVar)
                    continue;

                int index = Array.BinarySearch(varRvas, target);

                if (index < 0)
                    index = ~index - 1;

                if (index < 0)
                    continue;

                int offset = (int)(target - varRvas[index]);

                if (offset >= VAR_SPAN)
                    continue;

                if (xrefs[index] == null)
                    xrefs[index] = new List<(int, int)>();

                xrefs[index].Add((pos, offset));
            }

            bool crossesPadding(int a, int b)
            {
                int start = Math.Min(a, b), end = Math.Max(a, b);

                for (int i = textOffset + start; i < textOffset + end; i++)
                    if (binary[i] == 0xCC && binary[i + 1] == 0xCC)
                        return true;

                return false;
            }

            for (int i = 0; i < sorted.Length; i++)
            {
                var flag = sorted[i];
                var refs = xrefs[i] ?? new List<(int Pos, int Offset)>();
                long varOffset = getDataOffset(scan, flag.Variable, 32);

                if (varOffset < 0)
                {
                    result[flag.FullName] = null;
                    continue;
                }

                long length = BitConverter.ToInt64(binary, (int)varOffset + 16);

                if (length == 0)
                {
                    // mov qword ptr [rip+disp32], imm32 targeting the size field. The disp32 is followed by
                    // the immediate, so relative to the end of the disp32 the field appears 4 bytes earlier.
                    foreach (var xref in refs)
                    {
                        int pos = textOffset + xref.Pos;

                        if (xref.Offset != 12 || pos < 3)
                            continue;

                        if (binary[pos - 3] != 0x48 || binary[pos - 2] != 0xC7 || binary[pos - 1] != 0x05)
                            continue;

                        uint imm = BitConverter.ToUInt32(binary, pos + 4);

                        if (imm != 0)
                        {
                            length = imm;
                            break;
                        }
                    }
                }

                if (length == 0)
                {
                    result[flag.FullName] = "";
                    continue;
                }

                if (length > 4096)
                {
                    result[flag.FullName] = null;
                    continue;
                }

                int len = (int)length;

                if (len <= 15)
                {
                    bool isStatic = true;

                    for (int j = 0; j < len && isStatic; j++)
                        isStatic = isPrintable(binary[varOffset + j]);

                    if (isStatic)
                    {
                        result[flag.FullName] = Encoding.ASCII.GetString(binary, (int)varOffset, len);
                        continue;
                    }
                }

                string best = null;
                int bestDist = int.MaxValue;

                foreach (var xref in refs)
                {
                    int start = Math.Max(0, xref.Pos - SEARCH_WINDOW);
                    int end = Math.Min(textLength - 4, xref.Pos + SEARCH_WINDOW);

                    for (int pos = start; pos < end; pos++)
                    {
                        int dist = Math.Abs(pos - xref.Pos);

                        if (dist >= bestDist)
                            continue;

                        string literal = readRDataString(scan, ripTarget(pos), len);

                        if (literal == null || crossesPadding(pos, xref.Pos))
                            continue;

                        best = literal;
                        bestDist = dist;
                    }
                }

                result[flag.FullName] = best;
            }

            return result;
        }

        private static CppFlagScan ScanCppFlags(string studioExePath)
        {
            byte[] binary = File.ReadAllBytes(studioExePath);
            var exe = new PeFile(binary);

            ulong imageBase = exe.ImageNtHeaders.OptionalHeader.ImageBase;
            var sectionHeaders = exe.ImageSectionHeaders;

            var text = sectionHeaders.First(x => x.Name == ".text");
            var rdata = sectionHeaders.First(x => x.Name == ".rdata");
            var data = sectionHeaders.First(x => x.Name == ".data");

            ulong rdataStart = imageBase + rdata.VirtualAddress;
            ulong rdataEnd = rdataStart + rdata.VirtualSize;

            ulong dataStart = imageBase + data.VirtualAddress;
            ulong dataEnd = dataStart + Math.Max(data.VirtualSize, data.SizeOfRawData);

            // Flags are no longer registered through per-flag code stubs. Instead, .rdata
            // contains a static table of 8-byte aligned descriptors for each flag:
            //
            // 00:  const char* name     ; absolute pointer into .rdata
            // 08:  size_t      nameLen  ;
            // 10:  void*       variable ; absolute pointer into .data
            // 18:  uint32      scope    ; 1 = F, 2 = DF, 4 = SF
            // 1C:  uint32      type     ; 0/1 = Flag, 2 = Int, 3 = String, 4 = Log
            //
            // Versioned flags additionally have a record somewhere in the same region:
            //
            // 00:  const char* name     ;
            // 08:  size_t      nameLen  ;
            // 10:  size_t      version  ; small integer, appended to the flag name
            //
            // The variable holds the flag's engine default: a bool/int32/byte in the first field for
            // Flag/Int/Log, or an MSVC std::string for String.

            var scopes = new Dictionary<uint, string>
            {
                { 1, "F" },
                { 2, "DF" },
                { 4, "SF" },
            };

            var types = new Dictionary<uint, string>
            {
                { 0, "Flag" },
                { 1, "Flag" },
                { 2, "Int" },
                { 3, "String" },
                { 4, "Log" },
            };

            int rdataOffset = (int)rdata.PointerToRawData;
            int rdataLength = (int)Math.Min(rdata.SizeOfRawData, (uint)(binary.Length - rdataOffset)) & ~7;
            int rdataLimit = rdataOffset + rdataLength - 32;

            string readName(ulong ptr, ulong length)
            {
                if (ptr < rdataStart || ptr >= rdataEnd || length == 0 || length > 256)
                    return null;

                long offset = (long)(ptr - imageBase - rdata.VirtualAddress) + rdataOffset;

                if (offset + (long)length >= binary.Length)
                    return null;

                for (long i = offset; i < offset + (long)length; i++)
                {
                    byte value = binary[i];

                    bool isWordChar = (value >= '0' && value <= '9')
                                   || (value >= 'A' && value <= 'Z')
                                   || (value >= 'a' && value <= 'z')
                                   || value == '_';

                    if (isWordChar)
                        continue;

                    return null;
                }

                if (binary[offset + (long)length] != 0)
                    return null;

                return Encoding.ASCII.GetString(binary, (int)offset, (int)length);
            }

            var flags = new Dictionary<string, CppFlagEntry>();
            var versions = new Dictionary<string, ulong>();

            for (int pos = rdataOffset; pos <= rdataLimit; pos += 8)
            {
                ulong namePtr = BitConverter.ToUInt64(binary, pos);

                if (namePtr < rdataStart || namePtr >= rdataEnd)
                    continue;

                ulong nameLen = BitConverter.ToUInt64(binary, pos + 8);

                if (nameLen == 0 || nameLen > 256)
                    continue;

                ulong third = BitConverter.ToUInt64(binary, pos + 16);

                if (third >= dataStart && third < dataEnd)
                {
                    uint scope = BitConverter.ToUInt32(binary, pos + 24);
                    uint type = BitConverter.ToUInt32(binary, pos + 28);

                    if (!scopes.TryGetValue(scope, out string scopePrefix))
                        continue;

                    if (!types.TryGetValue(type, out string typeName))
                        continue;

                    string name = readName(namePtr, nameLen);

                    if (name != null)
                    {
                        flags[name] = new CppFlagEntry
                        {
                            Name = name,
                            FullName = scopePrefix + typeName + name,
                            Type = type,
                            Variable = third,
                        };
                    }
                }
                else if (third > 0 && third < 0x10000)
                {
                    string name = readName(namePtr, nameLen);

                    if (name == null)
                        continue;

                    if (!versions.TryGetValue(name, out ulong existing) || third > existing)
                    {
                        versions[name] = third;
                    }
                }
            }

            foreach (var flag in flags.Values)
                if (versions.TryGetValue(flag.Name, out ulong version) && version > 1)
                    flag.FullName += version;

            return new CppFlagScan
            {
                Binary = binary,
                ImageBase = imageBase,
                Text = text,
                RData = rdata,
                Data = data,
                Flags = flags.Values.ToList(),
            };
        }
    }
}
