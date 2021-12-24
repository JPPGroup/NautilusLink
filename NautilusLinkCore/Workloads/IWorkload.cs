using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TLS.NautilusLinkCore.Workloads
{
    public interface IWorkload
    {
        Task Run(ILogger<IWorkload> logger);
    }
}
