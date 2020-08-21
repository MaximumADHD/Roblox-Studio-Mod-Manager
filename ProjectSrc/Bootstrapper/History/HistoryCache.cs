using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace RobloxStudioModManager
{
    public class HistoryCache
    {
        private static readonly Dictionary<string, HistoryCache> BranchCache = new Dictionary<string, HistoryCache>();

        public string Branch { get; private set; }
        public string History { get; private set; }
        public DateTime LastUpdate { get; private set; }

        private HistoryCache(string branch)
        {
            Branch = branch;
            LastUpdate = DateTime.FromFileTimeUtc(0);

            BranchCache.Add(branch, this);
        }

        public static async Task<string> GetDeployHistory(string branch)
        {
            var deployHistory = Task.Run(() =>
            {
                HistoryCache cache = null;

                if (BranchCache.ContainsKey(branch))
                    cache = BranchCache[branch];
                else
                    cache = new HistoryCache(branch);

                lock (cache)
                {
                    TimeSpan timeDiff = DateTime.Now - cache.LastUpdate;

                    if (timeDiff.TotalMinutes > 5)
                    {
                        string historyEndpoint = $"https://s3.amazonaws.com/setup.{branch}.com/DeployHistory.txt";

                        using (WebClient http = new WebClient())
                            cache.History = http.DownloadString(historyEndpoint);

                        cache.LastUpdate = DateTime.Now;
                    }

                    return cache.History;
                }
            });

            return await deployHistory.ConfigureAwait(false);
        }
    }
}