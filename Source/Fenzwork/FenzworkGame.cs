﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fenzwork.Services;

namespace Fenzwork
{
    public static class FenzworkGame
    {
        public static string LongName { get; internal set; }
        public static string ShortName { get; internal set; }

        public static void Run(GameCore core, string LongName, string ShortName)
        {
            using MGGame mg = new(core);

            mg.Run();
        }

    }
}
