using HbmBrandSites.ContentPermissions.Drivers;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HbmBrandSites.ContentPermissions
{
    public class AdminMenu : INavigationProvider
    {
        public AdminMenu(IStringLocalizer<AdminMenu> localizer)
        {
            T = localizer;
        }

        public IStringLocalizer T { get; set; }
        public ValueTask BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (!String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return ValueTask.CompletedTask;
            }

            builder.Add(T["Content Permission Settings"], settings => settings
           .AddClass("content-permission")
           .Id("content-permission")
           .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = ContentPermissionSettingDisplayDriver.GroupId })
           .Permission(Permissions.ManageContentPermissions)
           .LocalNav());

            return ValueTask.CompletedTask;
        }
    }
}
