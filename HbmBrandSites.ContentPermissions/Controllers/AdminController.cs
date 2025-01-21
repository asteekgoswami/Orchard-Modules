using HbmBrandSites.ContentPermissions.Models;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace HbmBrandSites.ContentPermissions.Controllers
{
    [Admin]
    public class AdminController : Controller
    {
        private readonly ISiteService _siteService;
        private readonly INotifier _notifier;

        public AdminController(ISiteService siteService, INotifier notifier)
        {
            _siteService = siteService;
            _notifier = notifier;
        }

        public async Task<IActionResult> Index()
        {
            var siteSettings = await _siteService.GetSiteSettingsAsync();
            var settings = siteSettings.As<ContentPermissionSettings>() ?? new ContentPermissionSettings();
            return View(settings);
        }

        [HttpPost]
        public async Task<IActionResult> Save(ContentPermissionSettings model)
        {
            var siteSettings = await _siteService.GetSiteSettingsAsync();

            // Update settings using the Properties dictionary
            siteSettings.Properties[nameof(ContentPermissionSettings)] = JObject.FromObject(model);

            await _siteService.UpdateSiteSettingsAsync(siteSettings);

            TempData["Message"] = "Settings saved successfully.";
            return RedirectToAction(nameof(Index));
        }

    }
}
