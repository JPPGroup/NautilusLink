using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TLS.NautilusLinkCore.Workloads
{
    internal interface IWorkload
    {
        void Run(ILogger logger);
    }
}
