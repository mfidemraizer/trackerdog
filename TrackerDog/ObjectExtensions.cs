using Newtonsoft.Json;
using System;
using System.Dynamic;
using TrackerDog.Serialization.Json;

namespace TrackerDog
{
    internal static class ObjectExtensions
    {
        public static object CloneIt(this object some, Type type)
        {
            JsonSerializerSettings serializerSettings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new DynamicObjectContractResolver()
            };

            if (some is IDynamicMetaObjectProvider)
                serializerSettings.Converters.Add(new DynamicObjectWithDeclaredPropertiesConverter());

            string json = JsonConvert.SerializeObject(some, serializerSettings);

            return JsonConvert.DeserializeObject(json, type, serializerSettings);
        }
    }
}