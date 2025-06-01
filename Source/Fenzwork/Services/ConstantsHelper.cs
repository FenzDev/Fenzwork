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
        public static object GetGameInfo(string constName)
        {
            var classFullName = "Fenzwork._AutoGen.GameInfo";

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
        public static List<string> GetAssetsWorkingDirectories()
        {
            const string baseNamespace = "Fenzwork._AutoGen.";
            const string className = "AssetsInfo";
            const string constName = "WorkingDir";

            var results = new List<string>();

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                Type[] types;

                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    types = ex.Types.Where(t => t != null).ToArray()!;
                }

                foreach (var type in types)
                {
                    if (type == null)
                        continue;

                    // Match full namespace prefix and class name exactly
                    if (type.IsClass &&
                        type.IsAbstract &&
                        type.IsSealed && // static class
                        type.Name == className &&
                        type.Namespace != null &&
                        type.Namespace.StartsWith(baseNamespace, StringComparison.Ordinal))
                    {
                        const BindingFlags flags = BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy;

                        var field = type.GetField(constName, flags);
                        if (field == null)
                            continue;

                        if (!field.IsLiteral || field.IsInitOnly)
                            continue; // Skip if not a compile-time constant

                        if (field.FieldType == typeof(string))
                        {
                            var value = field.GetRawConstantValue() as string;
                            if (!string.IsNullOrEmpty(value))
                                results.Add(value);
                        }
                    }
                }
            }

            return results;
        }
    }
}