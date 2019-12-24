using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RobloxStudioModManager
{
    public class StudioDeployLogs
    {
        private const string LogPattern = "New (Studio6?4?) (version-[a-f\\d]+) at (\\d+/\\d+/\\d+ \\d+:\\d+:\\d+ [A,P]M), file version: (\\d+), (\\d+), (\\d+), (\\d+)...Done!";
        private const int EarliestChangelist = 338804; // The earliest acceptable changelist of Roblox Studio, with explicit 64-bit versions declared via DeployHistory.txt

        public string Branch { get; private set; }

        private string LastDeployHistory = "";
        private static Dictionary<string, StudioDeployLogs> LogCache = new Dictionary<string, StudioDeployLogs>();

        public HashSet<DeployLog> CurrentLogs_x86 = new HashSet<DeployLog>();
        public HashSet<DeployLog> CurrentLogs_x64 = new HashSet<DeployLog>();

        private StudioDeployLogs(string branch)
        {
            Branch = branch;
            LogCache[branch] = this;
        }

        private void MakeDistinct(HashSet<DeployLog> targetSet)
        {
            var byChangelist = new Dictionary<int, DeployLog>();
            var rejected = new List<DeployLog>();

            foreach (DeployLog log in targetSet)
            {
                int changelist = log.Changelist;

                if (byChangelist.ContainsKey(changelist))
                {
                    DeployLog oldLog = byChangelist[changelist];

                    if (oldLog.TimeStamp.CompareTo(log.TimeStamp) < 0)
                    {
                        byChangelist[changelist] = log;
                        rejected.Add(oldLog);
                    }
                }
                else
                {
                    byChangelist.Add(changelist, log);
                }
            }

            rejected.ForEach(log => targetSet.Remove(log));
        }

        private void UpdateLogs(string deployHistory, int maxVersion)
        {
            MatchCollection matches = Regex.Matches(deployHistory, LogPattern);
            CurrentLogs_x86.Clear();
            CurrentLogs_x64.Clear();

            foreach (Match match in matches)
            {
                string[] data = match.Groups.Cast<Group>()
                    .Select(group => group.Value)
                    .Where(value => value.Length != 0)
                    .ToArray();

                string buildType = data[1];
                bool is64Bit = buildType.EndsWith("64");

                DeployLog deployLog = new DeployLog()
                {
                    Is64Bit = is64Bit,
                    VersionGuid = data[2]
                };

                DateTime.TryParse(data[3], out deployLog.TimeStamp);

                int.TryParse(data[4], out deployLog.MajorRev);
                int.TryParse(data[5], out deployLog.Version);
                int.TryParse(data[6], out deployLog.Patch);
                int.TryParse(data[7], out deployLog.Changelist);

                if (deployLog.Changelist < EarliestChangelist || deployLog.Version > maxVersion)
                    continue;

                HashSet<DeployLog> targetList;

                if (deployLog.Is64Bit)
                    targetList = CurrentLogs_x64;
                else
                    targetList = CurrentLogs_x86;

                targetList.Add(deployLog);
            }

            MakeDistinct(CurrentLogs_x64);
            MakeDistinct(CurrentLogs_x86);
        }

        public static async Task<StudioDeployLogs> Get(string branch)
        {
            StudioDeployLogs logs = null;
            
            if (LogCache.ContainsKey(branch))
                logs = LogCache[branch];
            else
                logs = new StudioDeployLogs(branch);

            string deployHistory = await HistoryCache.GetDeployHistory(branch);

            if (logs.LastDeployHistory != deployHistory)
            {
                int maxVersion = int.MaxValue;

                if (branch == "roblox")
                {
                    string binaryType = StudioBootstrapper.GetStudioBinaryType();
                    var info = await ClientVersionInfo.Get(binaryType);

                    int version = info.Version
                        .Split('.')
                        .Select(int.Parse)
                        .Skip(1)
                        .First();

                    maxVersion = version;
                }

                logs.LastDeployHistory = deployHistory;
                logs.UpdateLogs(deployHistory, maxVersion);
            }

            return logs;
        }
    }
}
