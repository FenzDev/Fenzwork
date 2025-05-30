using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Fenzwork.Systems.Assets
{
    public readonly struct AssetID
    {
        public AssetID(string domain, string assetName, Type assetType)
        {
            Domain = domain;
            AssetName = assetName;
            AssetType = assetType;

            _Hash = HashCode.Combine(Domain, AssetName, AssetType);
        }
        public readonly string Domain;
        public readonly string AssetName;
        public readonly Type AssetType;
        readonly int _Hash;

        public override int GetHashCode() => _Hash;

        public override bool Equals([NotNullWhen(true)] object obj)
        {
            if (obj is AssetID idObj)
                return GetHashCode() == obj.GetHashCode() 
                    && AssetName == idObj.AssetName
                    && AssetType == idObj.AssetType
                    && Domain == idObj.Domain;

            return false;
        }

        public static bool operator ==(AssetID a, AssetID b) => a.Equals(b); 
        public static bool operator !=(AssetID a, AssetID b) => !a.Equals(b);

        public static implicit operator string(AssetID id) => id.ToString();

        public override string ToString() => Domain != "" ? $"{AssetName} ({AssetType.Name}) : {Domain}" : $"{AssetName} ({AssetType.Name})";
    }
}
