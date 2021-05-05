using System.Collections.Generic;
using TLS.Nautilus.Api.Shared.DataStructures;

namespace TLS.NautilusLink.ViewModels
{
    public class SiteSelectViewModel
    {
        public IEnumerable<SiteDefinition> Definitions { get; set; }

        public SiteDefinition Selected { get; set; }
    }
}
