using QuakeNavSharp.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace QuakeNavSharp.Json
{
    public abstract class NavJsonBase
    {
        public class MapInfo
        {
            public string Name { get; set; }
            public string Author { get; set; }
            public string Filename { get; set; }
            public string[] Urls { get; set; }
        }


        internal static JsonSerializerOptions _serializerOptions;

        static NavJsonBase()
        {
            _serializerOptions = new JsonSerializerOptions();
            _serializerOptions.Converters.Add(new Vector3JsonConverter());

            _serializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;

            _serializerOptions.WriteIndented = true;
            _serializerOptions.IgnoreNullValues = true;
        }

        public abstract int Version { get; }
        public abstract MapInfo Map { get; set; }
        public abstract string[] Contributors { get; set; }
        public abstract string Comments { get; set; }

        public abstract string ToJson();

        public abstract NavigationGraphBase ToNavigationGraphGeneric();
    }
}
