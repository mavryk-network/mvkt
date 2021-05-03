﻿using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tzkt.Api
{
    [JsonConverter(typeof(JsonStringConverter))]
    public class RawJson
    {
        public string Json { get; }
        public RawJson(string json) => Json = json;

        public static implicit operator RawJson (string value) => new RawJson(value);
        public static explicit operator string (RawJson value) => value.Json;
    }

    class JsonStringConverter : JsonConverter<RawJson>
    {
        public override RawJson Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, RawJson value, JsonSerializerOptions options)
        {
            using var doc = JsonDocument.Parse(value.Json, new JsonDocumentOptions { MaxDepth = 1024 });
            doc.WriteTo(writer);
        }
    }
}