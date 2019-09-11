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

        private static Dictionary<string, StudioDeployLogs> LogCache = new Dictionary<string, StudioDeployLogs>();
        private string LastDeployHistory = "";

        public List<DeployLog> CurrentLogs;
        public bool Has64BitLogs = false;

        private StudioDeployLogs(string branch)
        {
            Branch = branch;
            LogCache[branch] = this;

            CurrentLogs = new List<DeployLog>();
        }

        private void UpdateLogs(string deployHistory)
        {
            MatchCollection matches = Regex.Matches(deployHistory, LogPattern);

            Has64BitLogs = false;
            CurrentLogs.Clear();
            
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

                if (deployLog.Is64Bit)
                    Has64BitLogs = true;

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
