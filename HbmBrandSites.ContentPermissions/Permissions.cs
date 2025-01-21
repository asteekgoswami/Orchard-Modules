using OrchardCore.Security.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HbmBrandSites.ContentPermissions
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageContentPermissions = new Permission("ManageContentPermissions", "Manage content permissions");

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            return Task.FromResult(new[] { ManageContentPermissions } as IEnumerable<Permission>);
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
            new PermissionStereotype
            {
                Name = "Administrator",
                Permissions = new[] { ManageContentPermissions }
            }
        };
        }
    }
}
