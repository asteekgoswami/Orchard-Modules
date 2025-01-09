using OrchardCore.ContentManagement;
using System;

namespace HbmBrandSites.ContentPermissions.Models
{
    public class ContentPermissionsPart : ContentPart
    {
        public bool Enabled { get; set; }
        public string[] Roles { get; set; } = Array.Empty<string>();
    }
}
