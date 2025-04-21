using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniMart.Infrastructure
{
    public static class Utility
    {
        public static string GenerateTraceId() => DateTime.Now.ToString("yyyyMMddhhmmssffff");
    }
}
