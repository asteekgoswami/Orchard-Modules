using Fluid.Values;
using Fluid;
using OrchardCore.ContentManagement;
using OrchardCore.Liquid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YesSql;
using OrchardCore.ContentManagement.Records;


namespace HbmBrandSites.ContentPermissions.Liquid
{
    
    public class GetContentByTypenandDisplayText : ILiquidFilter
    {
        private readonly ISession _session;

        public GetContentByTypenandDisplayText(ISession session)
        {
            _session = session;
        }

        public async ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, LiquidTemplateContext context)
        {
            // Get the content type and title from the arguments
            var contentType = input.ToStringValue();
            var title = arguments["displaytext"]?.ToStringValue();

            if (string.IsNullOrWhiteSpace(contentType) || string.IsNullOrWhiteSpace(title))
            {
                return NilValue.Instance;
            }

            // Query content items using YesSql session
            var matchingItem = await _session.Query< ContentItem ,ContentItemIndex>()
                .Where(x => x.ContentType == contentType && x.DisplayText == title)
                .FirstOrDefaultAsync();

           
            return matchingItem != null ? new ObjectValue(matchingItem) : NilValue.Instance;
        }

    }
}
