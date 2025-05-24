using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fenzwork.Services.MMF
{
    public class SourcesMMF : MMFHandler
    {
        public SourcesMMF()
        {
            FilePath = Path.Combine( Path.GetTempPath(), $"FWDT_{DirUniqueHash}_Sources" );
        }

        public override void Tick()
        {
            // TODO: Sources once Assets was made
        }
    }
}
