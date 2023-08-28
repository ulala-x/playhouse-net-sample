using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleApi
{
    using Microsoft.Extensions.DependencyInjection;
    using PlayHouse.Production;
    using System;

    public static class GlobalControlProvider
    {
        public static ISystemPanel SystemPanel;
        public static ISender Sender;

    }


}
