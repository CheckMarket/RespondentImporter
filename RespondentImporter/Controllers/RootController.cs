using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.OptionsModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace CheckMarket.RespondentImporter.Controllers
{
    public class RootController: Controller
    {
        internal IOptions<Models.CheckMarketApiKeys> apiKeys = null;
        internal Microsoft.Extensions.Caching.Memory.IMemoryCache cache;
        public RootController(IOptions<Models.CheckMarketApiKeys> ApiKeys, Microsoft.Extensions.Caching.Memory.IMemoryCache cache)
        {
            this.apiKeys = ApiKeys;
            this.cache = cache;
        }
    }
}
