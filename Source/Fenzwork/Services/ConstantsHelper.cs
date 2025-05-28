using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Fenzwork.Services
{
    class ConstantsHelper
    {
        public static object GetAutoGenConstantByName(string constName)
        {
            var classFullName = "Fenzwork._AutoGen.Constants";

            // 1) Find the Type in any loaded assembly
            var targetType = AppDomain.CurrentDomain
                .GetAssemblies()
                .Select(a => a.GetType(classFullName, throwOnError: false, ignoreCase: false))
                .FirstOrDefault(t => t != null);

            if (targetType == null)
                throw new ArgumentException(
                    $"Type '{classFullName}' not found in loaded assemblies.",
                    nameof(classFullName));

            // 2) Look for the public static field
            const BindingFlags flags =
                BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy;

            var field = targetType.GetField(constName, flags);
            if (field == null)
                throw new ArgumentException(
                    $"No public static field named '{constName}' on type '{classFullName}'.",
                    nameof(constName));

            // 3) Verify it’s a literal const (not just static readonly)
            if (!field.IsLiteral || field.IsInitOnly)
                throw new ArgumentException(
                    $"Field '{constName}' on '{classFullName}' is not a compile-time const.",
                    nameof(constName));

            // 4) Return the compile-time value
            return field.GetRawConstantValue()!;
        }
    }
}
