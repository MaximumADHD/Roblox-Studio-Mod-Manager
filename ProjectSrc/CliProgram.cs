using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace RobloxStudioModManager
{
    static class Program
    {
        public const string BaseUrl = "https://setup.rbxcdn.com";
        public const string BaseConfigUrl = "https://raw.githubusercontent.com/MaximumADHD/Roblox-Studio-Mod-Manager/main/Config/";
        public const StringComparison StringFormat = StringComparison.Ordinal;
        public static readonly CultureInfo Format = CultureInfo.InvariantCulture;
        public static readonly NumberFormatInfo NumberFormat = NumberFormatInfo.InvariantInfo;
        public static ModManagerState State { get; private set; }
        public static bool AllowUnsupportedVersions { get; private set; }

        static async Task<int> Main(string[] args)
        {
            if (args.Length == 0 || Array.Exists(args, arg => arg == "--help" || arg == "-h"))
            {
                Console.WriteLine("Roblox Studio Bootstrapper (CLI; never launches Studio)");
                Console.WriteLine("  --output <directory> [--channel <name>] [--version <0.release.patch.build>]");
                Console.WriteLine("  [--version-guid <version-guid>] [--force] [--remap-extra-content] [--allow-unsupported]");
                Console.WriteLine("Channel authentication is read from RBX_CHANNEL_TOKEN.");
                return 0;
            }

            try
            {
                string output = null;
                string channel = "LIVE";
                string version = "";
                string guid = "";

                bool force = false;
                bool remap = false;

                for (int i = 0; i < args.Length; i++)
                {
                    string option = args[i];

                    switch (option)
                    {
                        case "--force":
                            force = true;
                            break;
                        case "--remap-extra-content":
                            remap = true;
                            break;
                        case "--allow-unsupported":
                            AllowUnsupportedVersions = true;
                            break;
                        case "--output":
                        case "--channel":
                        case "--version":
                        case "--version-guid":
                            if (++i >= args.Length || args[i].StartsWith("--", StringComparison.Ordinal))
                                throw new ArgumentException("Missing value for " + option);
                            
                            if (option == "--output")
                                output = args[i];
                            else if (option == "--channel")
                                channel = args[i];
                            else if (option == "--version")
                                version = args[i];
                            else
                                guid = args[i];
                            
                            break;
                        default: throw new ArgumentException("Unknown option: " + option);
                    }
                }

                if (string.IsNullOrWhiteSpace(output))
                    throw new ArgumentException("--output is required.");

                if (version != "" && (!Version.TryParse(version, out var parsedVersion) || parsedVersion.Revision < 0))
                    throw new ArgumentException("--version must have four numeric components.");
                
                if (guid != "" && (!Regex.IsMatch(guid, "^version-[a-fA-F0-9]{16}$") || version == ""))
                    throw new ArgumentException("--version-guid requires a version-<16 hex digits> value and --version.");
                
                output = Path.GetFullPath(output);
                StudioBootstrapper.ResolveFilePath(output, ".mod-manager-state.json");
                
                Directory.CreateDirectory(output);
                string statePath = Path.Combine(output, ".mod-manager-state.json");
                
                State = File.Exists(statePath)
                    ? JsonConvert.DeserializeObject<ModManagerState>(File.ReadAllText(statePath))
                    : new ModManagerState();
                
                if (State == null || State.VersionData == null || State.ChannelData == null
                    || State.FileManifest == null || State.PackageManifest == null)
                    throw new InvalidDataException("Invalid bootstrapper state.");

                State.ChannelData.ChannelName = channel;
                State.ChannelData.ChannelToken = Environment.GetEnvironmentVariable("RBX_CHANNEL_TOKEN") ?? "";

                var bootstrapper = new StudioBootstrapper(State)
                {
                    OverrideStudioDirectory = output,
                    OverrideGuid = guid,
                    CanShutdownStudio = false,
                    GenerateMetadata = true,
                    ForceInstall = force,
                    RemapExtraContent = remap,
                    ApplyModManagerPatches = false,
                };

                bootstrapper.EchoFeed += Console.WriteLine;
                bootstrapper.StatusFeed += message => Console.WriteLine("[status] " + message);

                if (!await bootstrapper.Bootstrap(version))
                    return 1;

                State.ChannelData.ChannelToken = "";
                File.WriteAllText(statePath, JsonConvert.SerializeObject(State, Formatting.Indented));
                Console.WriteLine("Studio files are ready in " + output);

                return 0;
            }
            catch (Exception error)
            {
                Console.Error.WriteLine(error.Message);
                return 1;
            }
        }
    }
}