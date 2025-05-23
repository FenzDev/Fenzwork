using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fenzwork
{
    public static class Log
    {
        public static void Debug(params object[] msg)
        {
            var now = DateTime.Now;
            Debugger.Log(0, "dbg", $"[{now:HH:mm:ss.ff}]");
            foreach (var word in  msg)
            {
                Debugger.Log(0, "dbg", $" {word}");
            }
            Debugger.Log(0, "dbg", $"\n");
        }
    }
}
