using HbmBrandSites.ContentPermissions.Models;
using OrchardCore.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HbmBrandSites.ContentPermissions.Services
{
    public class ContentPermissionSettingService : IContentPermissionSettingService
    {
        private readonly ISiteService _siteService;
        public ContentPermissionSettingService(ISiteService siteService)
        {
            _siteService = siteService;
        }
        
        public async Task<ContentPermissionSettings> GetSettingsAsync()
        {
            var siteSettings = await _siteService.GetSiteSettingsAsync();
            return siteSettings.As<ContentPermissionSettings>();
        }
    }

    public interface IContentPermissionSettingService
    {
        Task<ContentPermissionSettings> GetSettingsAsync();
    }
}

