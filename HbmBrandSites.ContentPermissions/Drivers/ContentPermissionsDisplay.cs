using HbmBrandSites.ContentPermissions.Models;
using HbmBrandSites.ContentPermissions.Services;
using HbmBrandSites.ContentPermissions.ViewModels;
using Microsoft.AspNetCore.Http;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Security.Services;
using OrchardCore.Title.Models;
using Parlot.Fluent;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace HbmBrandSites.ContentPermissions.Drivers
{
    public class ContentPermissionsDisplay : ContentPartDisplayDriver<ContentPermissionsPart>
    {
        #region Dependencies

        private readonly IContentPermissionsService _contentPermissionsService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRoleService _roleService;
        private readonly IContentManager _contentManager;

        private readonly IContentPermissionSettingService _contentPermissionSettingService;


        #endregion

        #region Constructor

        public ContentPermissionsDisplay(IContentPermissionsService contentPermissionsService, IHttpContextAccessor httpContextAccessor, IRoleService roleService, IContentManager contentManager, IContentPermissionSettingService contentPermissionSettingService)
        {
            _contentPermissionsService = contentPermissionsService;
            _httpContextAccessor = httpContextAccessor;
            _roleService = roleService;
            _contentManager = contentManager;
            _contentPermissionSettingService = contentPermissionSettingService;
        }

        #endregion

        #region Overrides

        public override async Task<IDisplayResult> DisplayAsync(ContentPermissionsPart part, BuildPartDisplayContext context)
        {
            var article = part.ContentItem;
            var settings = await _contentPermissionsService.GetSettingsAsync(part);

            var rolBasedSettings = await _contentPermissionSettingService.GetSettingsAsync();
            if(context.DisplayType != "Detail")
            {
                return null;
            }

            //this check to show the article if they have already access that previously
            if(context.DisplayType == "Detail" && _contentPermissionsService.HaveAccessToContent(article))
            {
                return null;
            }

            if (context.DisplayType == "Detail" && _contentPermissionsService.CanAccess(article))
            {
                //Simply we store the mapping of the user with contentItemsId they view and when (in date time field) 
                //as a content item temporarily in "RoleBasedAccess" ContentType.


                // if the record for particular user corresponding to the particular content item exist for last 30 days then dont create the entry again
                if (_contentPermissionsService.AlreadyContentAccess(part)) 
                {
                    return null;
                }
                else
                {

                    // Log access to the content item with the user's information
                    var currentUser = _httpContextAccessor.HttpContext.User;
                    var userId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
                    var userRole = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(c => c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;

                    var userName = currentUser.Identity.Name;
                    var contentItemId = article.ContentItemId;
                    var contentType = part.ContentItem.ContentType;
                    var accessDate = DateTime.UtcNow;

                    var contentItem = await _contentManager.NewAsync("RoleBasedAccess");
                    contentItem.DisplayText = userName;
                    // Set the Title Part value
                    contentItem.Alter<TitlePart>(part => part.Title = userName);


                    // Set other fields (Text, Date, Another Text)
                    contentItem.Alter<RoleBasedAccess>(part => part.UserId.Text = userId);
                    contentItem.Alter<RoleBasedAccess>(part => part.Role.Text = userRole);
                    contentItem.Alter<RoleBasedAccess>(part => part.ContentItemId.Text = contentItemId);
                    contentItem.Alter<RoleBasedAccess>(part => part.ContentType.Text = contentType);
                    contentItem.Alter<RoleBasedAccess>(part => part.DateTime.Value = accessDate);

                    // Save replicated content item
                    await _contentManager.CreateAsync(contentItem);

                    return null;
                }
            }

            _httpContextAccessor.HttpContext.Response.StatusCode = 403;

            var redirectUrl = "/Error/403";

            if (settings.HasRedirectUrl)
            {
                redirectUrl = settings.RedirectUrl;

                if (!settings.RedirectUrl.StartsWith('/'))
                {
                    redirectUrl = $"/{redirectUrl}";
                }
            }

            _httpContextAccessor.HttpContext.Response.Redirect($"{_httpContextAccessor.HttpContext.Request.PathBase}{redirectUrl}", false);

            return null;
        }

        public override async Task<IDisplayResult> EditAsync(ContentPermissionsPart part, BuildPartEditorContext context)
        {
            var roles = await _roleService.GetRoleNamesAsync();

            return Initialize<ContentPermissionsPartEditViewModel>("ContentPermissionsPart_Edit", model =>
            {
                model.ContentPermissionsPart = part;
                model.Enabled = part.Enabled;
                model.PossibleRoles = roles.ToArray();
                model.Roles = part.Roles;
            });
        }

        public override async Task<IDisplayResult> UpdateAsync( ContentPermissionsPart model, UpdatePartEditorContext context)
        {
            
            
            await context.Updater.TryUpdateModelAsync(model, Prefix, m => m.Enabled, m => m.Roles);

            if (!model.Enabled)
            {
                model.Roles = Array.Empty<string>();
            }

            return Edit(model, context);
        }

        #endregion
    }
}
