using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FenzExt
{
    public static class FenzExtGame
    {
        public static void Run(GameCore core)
        {
            using (MGGame mg = new(core))
            {
                mg.Run();
            }
        }
    }
}
