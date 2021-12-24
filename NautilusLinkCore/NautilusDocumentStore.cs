using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Jpp.Ironstone.Core;
using Jpp.Ironstone.Core.Autocad;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace TLS.NautilusLinkCore
{
    public class NautilusDocumentStore : DocumentStore
    {
        public List<KeyValuePair<long, Guid>> TreeMappings { get; set; }
        
        public NautilusDocumentStore(Document doc, Type[] managerTypes, ILogger<CoreExtensionApplication> log, LayerManager lm, IConfiguration settings) : base(doc, managerTypes, log, lm, settings)
        {
        }
        
        protected override void Save()
        {
            SaveBinary("TreeMappings", TreeMappings);
            base.Save();
        }

        protected override void Load()
        {
            TreeMappings = LoadBinary<List<KeyValuePair<long, Guid>>>("TreeMappings");
            base.Load();
        }
    }
}
