using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RobloxStudioModManager
{
    public class StudioDeployLogs
    {
        private const string LogPattern = "New Studio (version-[a-f\\d]+) at \\d+/\\d+/\\d+ \\d+:\\d+:\\d+ [A,P]M, file version: (\\d+), (\\d+), (\\d+), (\\d+)";
        public string Branch { get; private set; }

        private static Dictionary<string, StudioDeployLogs> LogCache = new Dictionary<string, StudioDeployLogs>();
        private string LastDeployHistory = "";

        public List<DeployLog> CurrentLogs;

        private StudioDeployLogs(string branch)
        {
            Branch = branch;
            LogCache[branch] = this;

            CurrentLogs = new List<DeployLog>();
        }

        private void UpdateLogs(string deployHistory)
        {
            MatchCollection matches = Regex.Matches(deployHistory, LogPattern);
            CurrentLogs.Clear();

            foreach (Match match in matches)
            {
                string[] data = match.Groups.Cast<Group>()
                    .Select(group => group.Value)
                    .Where(value => value.Length != 0)
                    .ToArray();

                DeployLog deployLog = new DeployLog();
                deployLog.VersionGuid = data[1];

                int.TryParse(data[2], out deployLog.MajorRev);
                int.TryParse(data[3], out deployLog.Version);
                int.TryParse(data[4], out deployLog.Patch);
                int.TryParse(data[5], out deployLog.Changelist);

                CurrentLogs.Add(deployLog);
            }
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
                logs.LastDeployHistory = deployHistory;
                logs.UpdateLogs(deployHistory);
            }

            return logs;
        }
    }
}
