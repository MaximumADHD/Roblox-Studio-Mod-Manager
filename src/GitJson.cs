using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace RobloxStudioModManager
{
    public static class JSONSerializer<T> where T : class
    {
        public static string Serialize(T instance)
        {
            var serializer = new DataContractJsonSerializer(typeof(T));
            using (MemoryStream stream = new MemoryStream())
            {
                serializer.WriteObject(stream, instance);
                return Encoding.Default.GetString(stream.ToArray());
            }
        }

        public static T DeSerialize(string json)
        {
            byte[] data = Encoding.Default.GetBytes(json);
            using (MemoryStream stream = new MemoryStream(data))
            {
                var serializer = new DataContractJsonSerializer(typeof(T));
                return serializer.ReadObject(stream) as T;
            }
        }
    }

    [DataContract]
    public class GitHubReleaseAsset
    {
        [DataMember]
        public string name;

        [DataMember]
        public string browser_download_url;
    }

    [DataContract]
    public class GitHubRelease
    {
        [DataMember]
        public GitHubReleaseAsset[] assets;

        public static GitHubRelease Get(string json)
        {
            return JSONSerializer<GitHubRelease>.DeSerialize(json);
        }
    }
}
