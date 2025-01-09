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

namespace HbmBrandSites.ContentPermissions
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IContentPermissionsService, ContentPermissionsService>();
            
            services.AddContentPart<ContentPermissionsPart>().UseDisplayDriver<ContentPermissionsDisplay>();


            services.AddScoped<IContentTypePartDefinitionDisplayDriver, ContentPermissionsPartSettingsDisplayDriver>();

            services.AddLiquidFilter<UserCanViewFilter>("user_can_view");

            services.AddScoped<IDataMigration, Migrations>();

            services.AddScoped<IContentPartIndexHandler, ContentPermissionsPartIndexHandler>();
        }
    }
}
