using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace QuakeNavSharp.Json
{
    public static class NavJsonUtils
    {
        private static Dictionary<uint, Type> _versionToTypeDictionary = new Dictionary<uint, Type>()
        {
            { 3, typeof(NavJson) },
            { 2, typeof(NavJsonV2) },
            { 1, typeof(NavJsonV1) }
        };

        public class HeaderStub
        {
            public uint Version { get; set; }
        }

        public static NavJsonBase LoadAnyVersion(string json)
        {
            var stub = JsonSerializer.Deserialize<HeaderStub>(json, NavJsonBase._serializerOptions);

            if (!_versionToTypeDictionary.TryGetValue(stub.Version, out var type))
                throw new InvalidDataException($"Unknown json version: {stub.Version}");

            return (NavJsonBase)JsonSerializer.Deserialize(json, type, NavJsonBase._serializerOptions);
        }
    }
}
