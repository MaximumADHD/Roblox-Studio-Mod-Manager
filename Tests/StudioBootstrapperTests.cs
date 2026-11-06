using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Konscious.Security.Cryptography;
using RobloxStudioModManager;
using Xunit;

namespace RobloxStudioModManager.Tests;

public class StudioBootstrapperTests
{
    [Fact]
    public async Task BootstrapInstallsFilesMatchingFetchedManifests()
    {
        const string version = "0.700.0.1";
        const string guid = "version-0123456789abcdef";
    
        const string configBase = "https://raw.githubusercontent.com/MaximumADHD/Roblox-Studio-Mod-Manager/main/Config/";
        const string deploymentBase = "https://setup.rbxcdn.com/channel/common/" + guid + "-";

        byte[] shared = Encoding.UTF8.GetBytes("shared content");
        byte[] binary = Encoding.UTF8.GetBytes("studio executable");
        byte[] inferred = Encoding.UTF8.GetBytes("inferred root");
        byte[] extra = Encoding.UTF8.GetBytes("unlisted package entry");
        byte[] ignored = Encoding.UTF8.GetBytes("ignored");

        var manifestFiles = new Dictionary<string, byte[]>
        {
            ["RobloxStudioBeta.exe"] = binary,
            [@"content\assets\nested\shared.txt"] = shared,
            [@"content\assets\copy.txt"] = shared,
            [@"Inferred\anchor.txt"] = inferred,
        };

        string fileManifest = string.Concat(manifestFiles.Select(pair => pair.Key + "\n" + Signature(pair.Value) + "\n"));

        byte[] assetsZip = Zip(new Dictionary<string, byte[]>
        {
            ["nested/shared.txt"] = shared,
            ["RobloxStudioBeta.exe"] = binary,
            ["ignored.luarc"] = ignored,
        });

        byte[] inferredZip = Zip(new Dictionary<string, byte[]>
        {
            ["extra.dat"] = extra,
            ["anchor.txt"] = inferred,
        });

        string packageManifest = "v0\n"
            + $"Assets.zip\nassets-signature\n{assetsZip.Length}\n{shared.Length + binary.Length + ignored.Length}\n"
            + $"Unknown.zip\nunknown-signature\n{inferredZip.Length}\n{extra.Length + inferred.Length}\n";

        var responses = new Dictionary<string, byte[]>
        {
            [configBase + "KnownRoots.json"] = Encoding.UTF8.GetBytes("{\"Assets\":{\"ExtractTo\":\"content/assets\",\"MinVersion\":0}}"),
            [configBase + "AppSettings.xml"] = Encoding.UTF8.GetBytes("<Settings />"),
            [configBase + "OAuth2Config.json"] = Encoding.UTF8.GetBytes("{}"),
            [deploymentBase + "rbxManifest.txt"] = Encoding.UTF8.GetBytes(fileManifest),
            [deploymentBase + "rbxPkgManifest.txt"] = Encoding.UTF8.GetBytes(packageManifest),
            [deploymentBase + "Assets.zip"] = assetsZip,
            [deploymentBase + "Unknown.zip"] = inferredZip,
        };

        using var handler = new FixtureHandler(responses);
        string output = Path.Combine(Path.GetTempPath(), "StudioBootstrapperTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(output);

        try
        {
            var state = new ModManagerState();

            var bootstrapper = new StudioBootstrapper(state, () => new HttpClient(handler, false))
            {
                OverrideStudioDirectory = output,
                OverrideGuid = guid,
                CanShutdownStudio = false,
            };

            bootstrapper.EchoFeed += _ => { };
            Assert.True(await bootstrapper.Bootstrap(version));
    
            Assert.Equal(version, state.VersionData.Version);
            Assert.Equal(guid, state.VersionData.VersionGuid);

            string[] manifestLines = Encoding.UTF8
                .GetString(responses[deploymentBase + "rbxManifest.txt"])
                .Split('\n', StringSplitOptions.RemoveEmptyEntries);

            Assert.Equal(manifestFiles.Count * 2, manifestLines.Length);

            for (int i = 0; i < manifestLines.Length; i += 2)
            {
                string relativePath = manifestLines[i];
                string expectedSignature = manifestLines[i + 1];
                string path = Path.Combine(output, relativePath);

                Assert.True(File.Exists(path), $"Missing manifest file: {relativePath}");
                Assert.Equal(expectedSignature, Signature(File.ReadAllBytes(path)));
                Assert.Equal(expectedSignature, state.FileManifest[relativePath]);
            }

            Assert.Equal(extra, File.ReadAllBytes(Path.Combine(output, "Inferred", "extra.dat")));
            Assert.Equal(Signature(extra), state.FileManifest[@"Inferred\extra.dat"]);
            Assert.False(File.Exists(Path.Combine(output, "content", "assets", "ignored.luarc")));
            Assert.Equal(assetsZip, File.ReadAllBytes(Path.Combine(output, "downloads", "Assets.zip")));
            Assert.Equal(inferredZip, File.ReadAllBytes(Path.Combine(output, "downloads", "Unknown.zip")));
            Assert.Equal("assets-signature", state.PackageManifest["Assets.zip"].Signature);
            Assert.Equal("unknown-signature", state.PackageManifest["Unknown.zip"].Signature);
            Assert.Equal(3, state.PackageManifest["Assets.zip"].NumFiles);
            Assert.Equal(2, state.PackageManifest["Unknown.zip"].NumFiles);
            Assert.Equal("<Settings />", File.ReadAllText(Path.Combine(output, "AppSettings.xml")));
            Assert.Equal("{}", File.ReadAllText(Path.Combine(output, "ApplicationConfig", "OAuth2Config.json")));

            string[] expectedFiles = manifestFiles.Keys
                .Concat(new string[] {
                    @"Inferred\extra.dat",
                    @"downloads\Assets.zip",
                    @"downloads\Unknown.zip",
                    "AppSettings.xml",
                    @"ApplicationConfig\OAuth2Config.json",
                })
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            string[] actualFiles = Directory.EnumerateFiles(output, "*", SearchOption.AllDirectories)
                .Select(path => Path.GetRelativePath(output, path))
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .ToArray();
            
            Assert.Equal(expectedFiles, actualFiles);

            foreach (string url in responses.Keys)
                Assert.Contains((HttpMethod.Get, url), handler.Requests);
            
            Assert.Contains((HttpMethod.Head, deploymentBase + "Assets.zip"), handler.Requests);
            Assert.Contains((HttpMethod.Head, deploymentBase + "Unknown.zip"), handler.Requests);
        }
        finally
        {
            Directory.Delete(output, true);
        }
    }

    private static string Signature(byte[] data)
    {
        using var hash = new HMACBlake2B(128);
        return Convert.ToHexStringLower(hash.ComputeHash(data));
    }

    private static byte[] Zip(Dictionary<string, byte[]> files)
    {
        using var stream = new MemoryStream();
        
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
        {
            foreach (var pair in files)
            {
                using var entry = archive.CreateEntry(pair.Key).Open();
                entry.Write(pair.Value);
            }
        }

        return stream.ToArray();
    }

    private sealed class FixtureHandler : HttpMessageHandler
    {
        private readonly Dictionary<string, byte[]> responses;
        public readonly ConcurrentBag<(HttpMethod Method, string Url)> Requests = new();

        public FixtureHandler(Dictionary<string, byte[]> responses) => this.responses = responses;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string url = request.RequestUri.AbsoluteUri;
            Requests.Add((request.Method, url));
            
            if (!responses.TryGetValue(url, out byte[] body))
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(request.Method == HttpMethod.Head ? Array.Empty<byte>() : body),
            });
        }
    }
}
