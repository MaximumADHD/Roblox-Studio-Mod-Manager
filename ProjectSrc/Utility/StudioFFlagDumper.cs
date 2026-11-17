// Forked from:
// https://github.com/pizzaboxer/rbxfflagdumper/blob/main/RbxFFlagDumper.Lib/StudioFFlagDumper.cs

using PeNet;

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

            ulong imageBase = exe.ImageNtHeaders.OptionalHeader.ImageBase;
            var sectionHeaders = exe.ImageSectionHeaders;
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

            var flags = new Dictionary<string, string>();
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
                        flags[name] = scopePrefix + typeName;
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

            var finalList = new List<string>();

            foreach (var pair in flags)
            {
                string name = pair.Value + pair.Key;

                if (versions.TryGetValue(pair.Key, out ulong version))
                    if (version > 1)
                        name += version;

                finalList.Add(name);
            }

            finalList.Sort();
            return finalList;
        }
    }
}
