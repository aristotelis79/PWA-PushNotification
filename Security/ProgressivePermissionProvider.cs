using System.Collections.Generic;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace Nop.Plugin.Progressive.Web.App.Security
{
    public class ProgressivePermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord ProgressivePermissionRecord = new PermissionRecord { Name = "Plugins. Admin area. Access Progressive Web Notification", SystemName = "AccessProgressiveWebNotification", Category = "Plugin" };

        public IEnumerable<PermissionRecord> GetPermissions()
        {
            return new []
            {
                ProgressivePermissionRecord
            };
        }

        public IEnumerable<DefaultPermissionRecord> GetDefaultPermissions()
        {
            return new[]
            {
                new DefaultPermissionRecord
                {
                    CustomerRoleSystemName = ProgressiveAppSystemNames.ProgressiveRoleName,
                    PermissionRecords = new[]
                    {
                        ProgressivePermissionRecord
                    }
                },
            };
        }
    }
}