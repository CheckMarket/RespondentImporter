using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CheckMarket.RespondentImporter.lib
{
    public class RateLimiter
    {
        int RequestCount;
        DateTime? MostRecentExecution;

        public RateLimiter(int RequestCountPerSecond)
        {
            this.RequestCount = RequestCountPerSecond;
        }

        public void WaitForRate() {
            if (MostRecentExecution.HasValue) {
                while (DateTime.UtcNow.Subtract(MostRecentExecution.Value).TotalSeconds <= (1 / RequestCount)) {
                    System.Threading.Thread.Sleep(100);
                }
            }
            MostRecentExecution = DateTime.UtcNow;
            return;
        }
    }
}
