using HbmBrandSites.ContentPermissions.Models;
using HbmBrandSites.OrchardCore.ContentPermissions.Models;
using Microsoft.AspNetCore.Http;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Security;
using OrchardCore.Settings;
using OrchardCore.Users.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using YesSql;
using ISession = YesSql.ISession;




namespace HbmBrandSites.ContentPermissions.Services
{

    public class ContentPermissionsService : IContentPermissionsService
    {
        #region Dependencies

        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly ISession _session;


        private readonly IContentPermissionSettingService _contentPermissionSettingService;

        #endregion

        #region Constructor

        public ContentPermissionsService(IContentDefinitionManager contentDefinitionManager, IHttpContextAccessor httpContextAccessor, IContentPermissionSettingService contentPermissionSettingService , ISession session)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _httpContextAccessor = httpContextAccessor;
            _contentPermissionSettingService = contentPermissionSettingService;
            _session = session;

        }

        #endregion

        #region Helpers

        public bool CanAccess(ContentItem contentItem)
        {
            return  CanAccess(contentItem.As<ContentPermissionsPart>()).GetAwaiter().GetResult();
        }

        public async Task<bool> CanAccess(ContentPermissionsPart part)
        {
            //Additional Logic
            var result = await GetUserContentAccessCountAsync(part);
            var accessCount = result.Item1;
            var limit = result.Item2;

            var isaccess = limit > accessCount ? true : false;
            

            if ((part == null || !part.Enabled || !part.Roles.Any()) && isaccess)
            {
                return true;
            }

            if (part.Roles.Contains("Anonymous") && isaccess)
            {
                return true;
            }

            if (_httpContextAccessor.HttpContext.User == null && isaccess)
            {
                return false;
            }

            if ((part.Roles.Contains("Authenticated") && _httpContextAccessor.HttpContext.User.Identity.IsAuthenticated) && isaccess)
            {
                return true;
            }

            foreach (var role in part.Roles)
            {
                if (_httpContextAccessor.HttpContext.User.IsInRole(role) && isaccess)
                {
                    return true;
                }
            }
            return false;
        }

        public async Task<ContentPermissionsPartSettings> GetSettingsAsync(ContentPermissionsPart part)
        {
            var contentTypeDefinition = await _contentDefinitionManager.GetTypeDefinitionAsync(part.ContentItem.ContentType);
            var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(x => string.Equals(x.PartDefinition.Name, nameof(ContentPermissionsPart)));
            return contentTypePartDefinition.GetSettings<ContentPermissionsPartSettings>();
        }

        //Function to count the acess of content in prev 30 days and return accessCount and limit of the content to their role
        public async Task<(int,int?)> GetUserContentAccessCountAsync(ContentPermissionsPart part)
        {
            var accessCount = 0;
            var roleBasedSettings = _contentPermissionSettingService.GetSettingsAsync().GetAwaiter().GetResult();

            //get the user details
            var userRole = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(c => c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;
            var userId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
            var contentType = part.ContentItem.ContentType;
            // Get the current date and time
            var now = DateTime.UtcNow;
            // Calculate the date-time for 30 days ago
            var last30DaysStart = now.AddDays(-30);

            var contentItems = _session.Query<ContentItem, ContentItemIndex>().Where(index => index.ContentType == "RoleBasedAccess" && index.CreatedUtc >= last30DaysStart && index.Published == true).ListAsync().GetAwaiter().GetResult();

            foreach (var item in contentItems)
            {
                var roleBasedAccess = item.Content.RoleBasedAccess;
                if (roleBasedAccess.UserId?.Text == userId && roleBasedAccess.Role?.Text == userRole)
                {
                    accessCount++;
                }
            }
            //get the role based content limit
            // Initialize a variable to store the limit
            int? limit = null;


            // Safely retrieve the limit for the user's role
            if (!string.IsNullOrEmpty(userRole) && roleBasedSettings.RoleArticleLimits.ContainsKey(userRole))
            {
                limit = roleBasedSettings.RoleArticleLimits[userRole];
            }

            return (accessCount,limit);
        }

        public bool AlreadyContentAccess(ContentPermissionsPart part)
        {
            var alreadyExist = 0;
            var roleBasedSettings = _contentPermissionSettingService.GetSettingsAsync().GetAwaiter().GetResult();

            //get the user details
            var userRole = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(c => c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;
            var userId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
            var contentType = part.ContentItem.ContentType;
            var contentItemId = part.ContentItem.ContentItemId;
            // Get the current date and time
            var now = DateTime.UtcNow;
            // Calculate the date-time for 30 days ago
            var last30DaysStart = now.AddDays(-30);

            var contentItems = _session.Query<ContentItem, ContentItemIndex>().Where(index => index.ContentType == "RoleBasedAccess" && index.CreatedUtc >= last30DaysStart && index.Published == true).ListAsync().GetAwaiter().GetResult();

            foreach (var item in contentItems)
            {
                var roleBasedAccess = item.Content.RoleBasedAccess;
                if (roleBasedAccess.UserId?.Text == userId && roleBasedAccess.Role?.Text == userRole && roleBasedAccess.ContentItemId?.Text == contentItemId)
                {
                    alreadyExist++;
                    break;
                }
            }

            if (alreadyExist <= 0)
            {
                return false;
            }
            return true;
        }

        public bool HaveAccessToContent(ContentItem contentItem)
        {
            var contentItemId = contentItem.ContentItemId;

            var query = _session.Query< ContentItem, ContentItemIndex> ().Where(index=> index.ContentType == "RoleBasedAccess" && index.Published == true).ListAsync().GetAwaiter().GetResult();
            var userId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

            foreach (var item in query)
            {
                var roleBasedAccess = item.Content.RoleBasedAccess;
                if (roleBasedAccess.ContentItemId?.Text == contentItemId && roleBasedAccess.UserId?.Text == userId)
                {
                    return true;
                }
            }

            return false;
        }
        #endregion

    }

    public interface IContentPermissionsService
    {
        bool CanAccess(ContentItem contentItem);
        Task<bool> CanAccess(ContentPermissionsPart part);

        Task<(int,int?)> GetUserContentAccessCountAsync(ContentPermissionsPart part);

        bool AlreadyContentAccess(ContentPermissionsPart part);

        Task<ContentPermissionsPartSettings> GetSettingsAsync(ContentPermissionsPart part);

        bool HaveAccessToContent(ContentItem contextItem);
    }

}



