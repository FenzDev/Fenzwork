using Fenzwork.AssetsSystem.Loaders;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Fenzwork.Systems.Assets
{
    public record AssetID(string Domain, string AssetName, Type CategoryType)
    {
        public override string ToString()
        {
            var split = Domain == "" ? "" : ":";
            return $"{Domain}{split}{AssetName} ({CategoryType.Name})";
        }
    }

    public static class Assets
    {
        public static bool AllowMultipleDomains;
        public static string DefaultDomain = "";
        public static string MainAssetsPath { get; set; } = Path.Combine(GetTitleContainerLocation(), "Assets");
        public static Dictionary<Type, IAssetLoader> _Loaders = new();
        internal static List<IAssetHandle> _AssetsBank = new();

        internal static void Load(MGGame game)
        {

            _Loaders.Add(typeof(Texture2D), new TextureLoader(game.GraphicsDevice));
            _Loaders.Add(typeof(Effect), new EffectLoader(game.GraphicsDevice));

            LoadDomains(MainAssetsPath);
        }

        internal static void LoadDomains(string assetsFolder)
        {
            if (AllowMultipleDomains)
            {
                var dirs = Directory.GetDirectories(assetsFolder);
                foreach (var dir in dirs)
                {
                    LoadAssets(dir, Path.GetRelativePath(assetsFolder, dir));
                }
                return;
            }
            LoadAssets(assetsFolder, DefaultDomain);
        }

        private static void LoadAssets(string assetsPath, string domain)
        {
            foreach (var loader in _Loaders)
            {
                var getMethod = GetGeneric(loader.Key);
                var categoryPath = Path.Combine(assetsPath, loader.Value.CategoryName);

                if (!Directory.Exists(categoryPath))
                    continue;

                var files = loader.Value.FileExtensions.SelectMany(ext => Directory.GetFiles(categoryPath, $"*.{ext}", SearchOption.AllDirectories));
                foreach (var file in files)
                {
                    using (var reader = File.OpenRead(file))
                    {
                        var relativePath = Path.GetRelativePath(categoryPath, file);
                        var assetname = Path.ChangeExtension(relativePath, null).Replace('\\', '/');
                        var id = new AssetID(domain, assetname, loader.Key);
                        var content = loader.Value.Load($"{domain}:{assetname}", reader);

                        var asset = (IAssetHandle)getMethod.Invoke(null, [id]);

                        asset.Content = content;
                        asset.IsLoaded = true;
                    }
                }
            }
        }

        static MethodInfo GetGeneric(Type type)
        {
            var methods = typeof(Assets)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(m => m.Name == "Get" && m.IsGenericMethodDefinition)
                .ToList();

            // Pick the overload that has exactly one parameter of type AssetID
            var targetMethod = methods.First(m =>
            {
                var parameters = m.GetParameters();
                return parameters.Length == 1 && parameters[0].ParameterType == typeof(AssetID);
            });

            // Now create the generic version
            return targetMethod.MakeGenericMethod(type);

        }

        internal static void Unload(MGGame game)
        {
            _Loaders.Clear();

            foreach (var asset in _AssetsBank)
            {
                asset.IsLoaded = false;
                asset.Content = null;
            }

            _AssetsBank.Clear();
        }

        public static AssetHandle<T> Get<T>(string assetname) => Get<T>(assetname, "");
        public static AssetHandle<T> Get<T>(string assetname, string domain) => Get<T>(new AssetID(domain == "" ? DefaultDomain : domain, assetname, typeof(T)));

        public static AssetHandle<T> Get<T>(AssetID assetID)
        {
            var asset = (AssetHandle<T>)_AssetsBank.Find(e => e.ID == assetID);

            if (asset is not null)
                return asset;

            asset = new AssetHandle<T>() { ID = assetID, Content = (T)_Loaders[assetID.CategoryType].DefaultObject, IsLoaded = false };
            _AssetsBank.Add(asset);

            return asset;
        }

        private static string GetTitleContainerLocation()
        {
            // Get the TitleContainer type
            Type titleContainerType = typeof(TitleContainer);

            // Get the internal property Location using reflection
            PropertyInfo locationProperty = titleContainerType.GetProperty("Location", BindingFlags.NonPublic | BindingFlags.Static);

            // Get the value of the Location property (it's a static property)
            return (string)locationProperty.GetValue(null); // null because it's a static property
        }


    }
}
