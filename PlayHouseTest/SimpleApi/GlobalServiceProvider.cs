using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleApi
{
    using Microsoft.Extensions.DependencyInjection;
    using System;

    public static class GlobalServiceProvider
    {
        private static ServiceProvider? _instance;

        public static ServiceProvider Instance
        {
            get
            {
                if (_instance == null)
                    throw new InvalidOperationException("ServiceProvider has not been set.");
                return _instance;
            }
            set
            {
                if (_instance != null)
                    throw new InvalidOperationException("ServiceProvider has already been set.");
                _instance = value;
            }
        }

    }


}
