using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace TrackerDog.Serialization.Json
{
    internal sealed class DynamicObjectWithDeclaredPropertiesConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(IDynamicMetaObjectProvider).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            JObject jObject = JObject.Load(reader);

            var target = Activator.CreateInstance(objectType);

            JsonReader jObjectReader = jObject.CreateReader();
            jObjectReader.Culture = reader.Culture;
            jObjectReader.DateParseHandling = reader.DateParseHandling;
            jObjectReader.DateTimeZoneHandling = reader.DateTimeZoneHandling;
            jObjectReader.FloatParseHandling = reader.FloatParseHandling;

            serializer.Populate(jObjectReader, target);

            return target;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var properties = value.GetType()
                                    .GetProperties()
                                    .Where
                                    (
                                        p => p.PropertyType != typeof(DynamicObject)
                                             && p.GetIndexParameters().Length == 0
                                             && p.GetCustomAttribute<JsonIgnoreAttribute>() == null
                                    ).ToList();

            JObject o = (JObject)JToken.FromObject(value, serializer);

            foreach (PropertyInfo property in properties)
                if (o[property.Name] == null)
                    o.AddFirst(new JProperty(property.Name, property.GetValue(value)));

            o.WriteTo(writer);
        }
    }
}