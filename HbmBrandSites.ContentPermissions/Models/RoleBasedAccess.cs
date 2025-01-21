using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HbmBrandSites.ContentPermissions.Models
{
    public class RoleBasedAccess : ContentPart
    {
        public TextField UserId { get; set; }
        public TextField Role { get; set;  }
        public TextField ContentItemId { get; set; }
        public TextField ContentType { get; set; }
        public DateTimeField DateTime { get; set; }
    }
}
