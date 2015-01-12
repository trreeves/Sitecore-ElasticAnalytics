namespace ElasticAnalytics.Repository.Elasticsearch.Serialization
{
    using System;

    using ElasticAnalytics.Repository.Elasticsearch.Extensions;

    using Newtonsoft.Json;

    public class EsApiFriendlyGuidConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            string text;

            if (value is Guid)
            {
                var guid = (Guid)value;
                text = guid.ToEsSafeId(); // the line that matters - we write guids without hyphens!
            }
            else
            {
                throw new JsonSerializationException(string.Format(
                    "Unexpected value when converting Guid. Expected System.Guid, got {0}.", value.GetType().FullName));
            }

            writer.WriteValue(text);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            bool nullable = IsNullableType(objectType);

            Type t = (nullable) ?
                Nullable.GetUnderlyingType(objectType) :
                objectType;

            if (reader.TokenType == JsonToken.Null)
            {
                if (!IsNullableType(objectType))
                    throw new JsonSerializationException("Cannot convert null value to " + objectType.FullName);

                return null;
            }

            if (reader.TokenType != JsonToken.String)
                throw new JsonSerializationException("Unexpected token parsing guid. Expected String, got " + reader.TokenType);

            string guidText = reader.Value.ToString();

            if (string.IsNullOrEmpty(guidText) && nullable)
                return null;

            return Guid.Parse(guidText);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Guid) || objectType == typeof(Guid?);
        }

        private static bool IsNullableType(Type t)
        {
            return t.IsValueType && (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>));
        }
    }
}
