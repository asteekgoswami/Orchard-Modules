using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HbmBrandSites.ContentPermissions.Models
{
    public class ContentPermissionSettings
    {
        public Dictionary<string, int> RoleArticleLimits { get; set; } = new Dictionary<string, int>();

        // Dictionary to store role and whether article limit is enabled and its value
        //public Dictionary<string, (bool IsLimitEnabled, int ArticleLimit)> RoleArticleLimits { get; set; } = new Dictionary<string, (bool, int)>();
    }

    /* public class ContentPermissionSettings
     {
         public Dictionary<string, RoleArticleLimit> RoleArticleLimits { get; set; } = new Dictionary<string, RoleArticleLimit>();
     }

     public class RoleArticleLimit
     {
         public bool IsLimitEnabled { get; set; }
         public int ArticleLimit { get; set; }
     }*/


}
