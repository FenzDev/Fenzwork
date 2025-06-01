using Fenzwork.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fenzwork
{
    public static class Assets
    {
        public readonly static IReadOnlyCollection<string> DebugWorkingDirectories = ConstantsHelper.GetAssetsWorkingDirectories().AsReadOnly();

    }
}
