using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace QuakeNavSharp.Json
{
    internal class Vector3JsonConverter : JsonConverter<Vector3>
    {
        public override Vector3 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if(reader.TokenType != JsonTokenType.StartArray)
                throw new JsonException();

            reader.Read(); // Skip the array start

            var x = reader.GetSingle();
            reader.Read();

            var y = reader.GetSingle();
            reader.Read();
            
            var z = reader.GetSingle();
            reader.Read();

            if (reader.TokenType != JsonTokenType.EndArray)
                throw new JsonException();

            return new Vector3(x, y, z);
        }


        public override void Write(Utf8JsonWriter writer, Vector3 vector, JsonSerializerOptions options) {
            writer.WriteStartArray();
            
            writer.WriteNumberValue(vector.X);
            writer.WriteNumberValue(vector.Y);
            writer.WriteNumberValue(vector.Z);
            
            writer.WriteEndArray();
        }
    }
}
