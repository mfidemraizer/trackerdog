using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Dynamic;
using System.Linq;

namespace TrackerDog.Serialization.Json
{
    internal sealed class DynamicObjectWithDeclaredPropertiesConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return true;
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
            var properties = value.GetType().GetProperties().Where(x => x.PropertyType != typeof(DynamicObject)).ToList();
            JObject o = (JObject)JToken.FromObject(value);

            properties.ForEach(x =>
            {
                o.AddFirst(new JProperty(x.Name, x.GetValue(value)));
            });

            o.WriteTo(writer);
        }
    }
}