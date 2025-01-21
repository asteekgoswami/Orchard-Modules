using HbmBrandSites.ContentPermissions.Indexing;
using HbmBrandSites.ContentPermissions.Liquid;
using HbmBrandSites.ContentPermissions.Models;
using HbmBrandSites.ContentPermissions.Services;
using HbmBrandSites.ContentPermissions.Settings;
using HbmBrandSites.ContentPermissions.Drivers;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data.Migration;
using OrchardCore.Indexing;
using OrchardCore.Liquid;
using OrchardCore.Modules;
using YesSql.Indexes;
using OrchardCore.Navigation;
using OrchardCore.DisplayManagement.Handlers;

namespace HbmBrandSites.ContentPermissions
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddSiteDisplayDriver<ContentPermissionSettingDisplayDriver>();
            services.AddScoped<IContentPermissionsService, ContentPermissionsService>();
            
            services.AddContentPart<ContentPermissionsPart>().UseDisplayDriver<ContentPermissionsDisplay>();


            services.AddScoped<IContentTypePartDefinitionDisplayDriver, ContentPermissionsPartSettingsDisplayDriver>();

            services.AddLiquidFilter<UserCanViewFilter>("user_can_view");
            services.AddLiquidFilter<GetContentByTypenandDisplayText>("get_content_by_type_and_displaytext");

            services.AddScoped<IDataMigration, Migrations>();

            services.AddScoped<IContentPartIndexHandler, ContentPermissionsPartIndexHandler>();


            services.AddScoped<IContentPermissionSettingService, ContentPermissionSettingService>();
        }
    }
}
