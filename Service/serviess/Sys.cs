using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace serviess
{
    class Sys
    {
        public static long Getimetamp()
        {
            TimeSpan tsp = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0);
            return Convert.ToInt64(tsp.TotalSeconds);
        }
    }
}
