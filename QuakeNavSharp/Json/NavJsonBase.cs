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
        internal static JsonSerializerOptions _serializerOptions;

        static NavJsonBase()
        {
            _serializerOptions = new JsonSerializerOptions();
            _serializerOptions.Converters.Add(new Vector3JsonConverter());

            _serializerOptions.WriteIndented = true;
            _serializerOptions.IgnoreNullValues = true;
        }

        public abstract string ToJson();

        public abstract NavigationGraphBase ToNavigationGraphGeneric();
    }
}
