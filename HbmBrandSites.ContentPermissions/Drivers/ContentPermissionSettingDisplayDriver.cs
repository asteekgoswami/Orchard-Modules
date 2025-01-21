using HbmBrandSites.ContentPermissions.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Security.Services;
using OrchardCore.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HbmBrandSites.ContentPermissions.Drivers
{
    public class ContentPermissionSettingDisplayDriver : SectionDisplayDriver<ISite, ContentPermissionSettings>
    {
        public const string GroupId = "ContentPermission";
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthorizationService _authorizationService;
        private readonly IRoleService _roleService;
        public ContentPermissionSettingDisplayDriver(
            IHttpContextAccessor httpContextAccessor,
            IAuthorizationService authorizationService,
            IRoleService roleService)
        {
            _httpContextAccessor = httpContextAccessor;
            _authorizationService = authorizationService;
            _roleService = roleService;
        }

        public override async Task<IDisplayResult> EditAsync(ISite site, ContentPermissionSettings settings, BuildEditorContext context)
        {
            // Authorization check: Ensure the user has the required permissions
            var user = _httpContextAccessor.HttpContext?.User;
            if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageContentPermissions))
            {
                return null;
            }

            // Fetch roles asynchronously
            var roles = await _roleService.GetRoleNamesAsync();

            return Initialize<ContentPermissionSettings>("ContentPermissionSettings_Edit", model =>
            {
                model.RoleArticleLimits = settings.RoleArticleLimits;

                // Ensure all roles are present in the dictionary
                foreach (var role in roles.Where(role => !model.RoleArticleLimits.ContainsKey(role)))
                {
                    model.RoleArticleLimits[role] = 0;
                }
            })
            .Location("Content:2")
            .OnGroup(GroupId);
        }

        /* public override async Task<IDisplayResult> EditAsync(ISite site, ContentPermissionSettings settings, BuildEditorContext context)
         {
             var user = _httpContextAccessor.HttpContext?.User;

             if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageContentPermissions))
             {
                 return null;
             }

             var roles = await _roleService.GetRoleNamesAsync();

             // Initialize ContentPermissionSettings
             return Initialize<ContentPermissionSettings>("ContentPermissionSettings_Edit", model =>
             {
                 model.RoleArticleLimits = settings.RoleArticleLimits;

                 // Ensure all roles are present in RoleArticleLimits, defaulting to 0 and IsLimitEnabled = false
                 foreach (var role in roles.Where(role => !model.RoleArticleLimits.ContainsKey(role)))
                 {
                     model.RoleArticleLimits[role] = new RoleArticleLimit
                     {
                         IsLimitEnabled = false,  // Default to false
                         ArticleLimit = 0         // Default article limit
                     };
                 }

                 // Optionally, ensure existing roles in the model have defaults if they're missing any properties
                 foreach (var key in model.RoleArticleLimits.Keys.ToList())
                 {
                     var limit = model.RoleArticleLimits[key];
                     if (limit == null)
                     {
                         model.RoleArticleLimits[key] = new RoleArticleLimit
                         {
                             IsLimitEnabled = false,  // Default to false
                             ArticleLimit = 0         // Default article limit
                         };
                     }
                 }

             })
             .Location("Content:2")
             .OnGroup(GroupId);
         }*/


        public override async Task<IDisplayResult> UpdateAsync(ISite site, ContentPermissionSettings settings, UpdateEditorContext context)
        {
            // Authorization check: Ensure the user has the required permissions
            var user = _httpContextAccessor.HttpContext?.User;
            if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageContentPermissions))
            {
                return null;
            }

            if (context.GroupId.Equals(GroupId, StringComparison.OrdinalIgnoreCase))
            {
                // Updating settings logic
                await context.Updater.TryUpdateModelAsync(settings , Prefix , settings=> settings.RoleArticleLimits);

                // You can perform other updates here if needed.
            }

            return await EditAsync(site, settings, context);
        }

    }
}
