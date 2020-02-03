using System.Collections.Generic;

namespace Nop.Plugin.Progressive.Web.App.Security
{
    public class ProgressiveAppSystemNames
    {
        public static string ProgressiveRoleName = "ProgressiveRoleName";

        public static IEnumerable<string> GetSystemRoleNames()
        {
            return new List<string>{ProgressiveRoleName};
        }
    }
}