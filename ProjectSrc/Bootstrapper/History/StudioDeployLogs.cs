using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RobloxStudioModManager
{
    public class StudioDeployLogs
    {
        private const string LogPattern = "New (Studio6?4?) (version-[a-f\\d]+) at \\d+/\\d+/\\d+ \\d+:\\d+:\\d+ [A,P]M, file version: (\\d+), (\\d+), (\\d+), (\\d+)...Done!";

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

        private void UpdateLogs(string deployHistory)
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

                DeployLog deployLog = new DeployLog()
                {
                    BuildType = data[1],
                    VersionGuid = data[2]
                };

                int.TryParse(data[3], out deployLog.MajorRev);
                int.TryParse(data[4], out deployLog.Version);
                int.TryParse(data[5], out deployLog.Patch);
                int.TryParse(data[6], out deployLog.Changelist);

                HashSet<DeployLog> targetList;

                if (deployLog.Is64Bit)
                    targetList = CurrentLogs_x64;
                else
                    targetList = CurrentLogs_x86;

                targetList.Add(deployLog);
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
